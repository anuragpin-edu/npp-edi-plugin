using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;

// Edi.* types are referenced only inside method bodies — never in static field
// initialisers or static constructors — so they are not loaded until the first
// plugin command is actually invoked by the user.
using Edi.Core.Formatting;
using Edi.Core.Model;
using Edi.Core.Parsing;
using Edi.Edifact.Formatting;
using Edi.Edifact.Parsing;
using Edi.X12.Formatting;
using Edi.X12.Parsing;
using Edi.Dictionaries;
using NppEdiPlugin.Forms;

namespace Kbg.NppPluginNET
{
    class Main
    {
        internal const string PluginName = "NppEdiPlugin";

        // ── Fields that can be set only by Notepad++ calls — never initialised eagerly ──
        private static EdiTreeForm      _treeForm;
        private static int              _idMyDlg = -1;

        // Kept for toolbar registration; allocate on demand
        private static Bitmap _tbBmp;
        private static Bitmap _tbBmpTab;

        // Suppresses CS0414 on iniFilePath / tbIcon which were in original template
        #pragma warning disable 0414
        private static readonly string  _iniFilePath   = null;
        private static readonly Icon    _tbIcon        = null;
        #pragma warning restore 0414

        // ── Lazy EDI runtime objects — created on first use, not during setInfo ──
        private static EdiParserDispatcher    _parserDispatcher;
        private static EdiFormatterDispatcher _formatterDispatcher;
        private static JsonEdiDictionary      _dictionary;
        private static ScintillaGateway       _scintilla;

        // ─────────────────────────────────────────────────────────────────────
        // Lazy-accessor helpers
        // ─────────────────────────────────────────────────────────────────────
        private static ScintillaGateway GetScintilla()
        {
            if (_scintilla == null)
                _scintilla = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            return _scintilla;
        }

        private static EdiParserDispatcher GetParserDispatcher()
        {
            if (_parserDispatcher == null)
                _parserDispatcher = new EdiParserDispatcher(
                    new IEdiParser[] { new EdifactParser(), new X12Parser() });
            return _parserDispatcher;
        }

        private static EdiFormatterDispatcher GetFormatterDispatcher()
        {
            if (_formatterDispatcher == null)
                _formatterDispatcher = new EdiFormatterDispatcher(
                    new IEdiFormatter[] { new EdifactFormatter(), new X12Formatter() });
            return _formatterDispatcher;
        }

        private static JsonEdiDictionary GetDictionary()
        {
            if (_dictionary == null)
                _dictionary = new JsonEdiDictionary();
            return _dictionary;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Dummy properties to satisfy NppCSharpPluginPack Utils dependencies
        // ─────────────────────────────────────────────────────────────────────
        public static string PluginConfigDirectory     { get; set; }
        public static bool   isShuttingDown            = false;
        public static Form   selectionRememberingForm  = null;
        public static int    IdCloseHtmlTag            = -1;
        public static void   RestyleEverything()       { }
        internal static void SetToolBarIcons()         { SetToolBarIcon(); }

        // ─────────────────────────────────────────────────────────────────────
        // Notepad++ lifecycle
        // ─────────────────────────────────────────────────────────────────────
        public static void OnNotification(ScNotification notification)
        {
            // Hook into Notepad++ events here as needed
        }

        internal static void CommandMenuInit()
        {
            // Only register delegates — no Edi.* object construction here.
            PluginBase.SetCommand(0, "Parse EDI",          ParseEdi,        new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(1, "Prettify EDI",       PrettifyEdi,     new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(2, "Minify EDI",         MinifyEdi,       new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(3, "Toggle EDI Tree Panel", ToggleTreePanel, new ShortcutKey(false, false, false, Keys.None));
            _idMyDlg = 3;
            PluginBase.SetCommand(4, "---",                null);
            PluginBase.SetCommand(5, "About NppEdiPlugin", About);
        }

        internal static void SetToolBarIcon()
        {
            if (_tbBmp == null) _tbBmp = new Bitmap(16, 16);
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = _tbBmp.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle,
                              (uint)NppMsg.NPPM_ADDTOOLBARICON,
                              PluginBase._funcItems.Items[_idMyDlg]._cmdID,
                              pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }

        internal static void PluginCleanUp()
        {
            // Nothing to clean up yet
        }

        // ─────────────────────────────────────────────────────────────────────
        // Plugin commands — EDI runtime is constructed here, on first use only
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Reads the active Scintilla buffer and parses it using the generic parser dispatcher.
        /// Auto-detects the EDI standard. Shows results in the dockable tree panel.
        /// </summary>
        internal static void ParseEdi()
        {
            try
            {
                if (_treeForm == null || !_treeForm.Visible)
                    ToggleTreePanel();

                string text   = GetEditorText();
                var    doc    = GetParserDispatcher().Parse(text, GetDictionary());

                if (doc.Standard == EdiStandard.Unknown)
                {
                    MessageBox.Show(
                        "Could not detect a supported EDI standard in the current document.\n\n" +
                        "Supported standards: EDIFACT, X12",
                        PluginName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                _treeForm?.RenderDocument(doc);
            }
            catch (Exception ex)
            {
                HandleCommandException(ex, "Parse EDI");
            }
        }

        /// <summary>
        /// Reads the active Scintilla buffer, prettifies it, and replaces the buffer contents.
        /// </summary>
        internal static void PrettifyEdi()
        {
            try
            {
                string text   = GetEditorText();
                var    result = GetFormatterDispatcher().Prettify(text);

                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.ErrorMessage, PluginName,
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ReplaceEditorText(result.FormattedText);
            }
            catch (Exception ex)
            {
                HandleCommandException(ex, "Prettify EDI");
            }
        }

        /// <summary>
        /// Reads the active Scintilla buffer, minifies it, and replaces the buffer contents.
        /// </summary>
        internal static void MinifyEdi()
        {
            try
            {
                string text   = GetEditorText();
                var    result = GetFormatterDispatcher().Minify(text);

                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.ErrorMessage, PluginName,
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ReplaceEditorText(result.FormattedText);
            }
            catch (Exception ex)
            {
                HandleCommandException(ex, "Minify EDI");
            }
        }

        internal static void ToggleTreePanel()
        {
            try
            {
                if (_treeForm == null)
                {
                    if (_tbBmpTab == null) _tbBmpTab = new Bitmap(16, 16);
                    _treeForm = new EdiTreeForm(GetScintilla());

                    NppTbData nppTbData = new NppTbData();
                    nppTbData.hClient     = _treeForm.Handle;
                    nppTbData.pszName     = "EDI Inspector";
                    nppTbData.dlgID       = _idMyDlg;
                    nppTbData.uMask       = NppTbMsg.DWS_DF_CONT_LEFT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                    nppTbData.hIconTab    = (uint)_tbBmpTab.GetHicon();
                    nppTbData.pszModuleName = PluginName;

                    IntPtr ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(nppTbData));
                    Marshal.StructureToPtr(nppTbData, ptrNppTbData, false);
                    Win32.SendMessage(PluginBase.nppData._nppHandle,
                                      (uint)NppMsg.NPPM_DMMREGASDCKDLG,
                                      0, ptrNppTbData);
                    Marshal.FreeHGlobal(ptrNppTbData);
                }
                else
                {
                    Win32.SendMessage(PluginBase.nppData._nppHandle,
                                      (uint)NppMsg.NPPM_DMMSHOW,
                                      0, _treeForm.Handle);
                }
            }
            catch (Exception ex)
            {
                HandleCommandException(ex, "Toggle EDI Tree Panel");
            }
        }

        internal static void About()
        {
            MessageBox.Show(
                "NppEdiPlugin — EDI Inspector for Notepad++\n" +
                "Supports: EDIFACT, X12 (VDA planned)\n\n" +
                "Phase 3 — Multi-DLL packaging\n" +
                $"Bootstrap log: {BootstrapLogger.LogPath}",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helper methods
        // ─────────────────────────────────────────────────────────────────────

        private static string GetEditorText()
        {
            var sci    = GetScintilla();
            int length = sci.GetTextLength();
            return sci.GetText(length + 1);
        }

        private static void ReplaceEditorText(string newText)
        {
            var sci = GetScintilla();
            sci.SelectAll();
            sci.ReplaceSel(newText);
        }

        private static void HandleCommandException(Exception ex, string commandName)
        {
            string msg = $"An error occurred during '{commandName}':\n\n{ex.Message}";
            
            // Check for Mark of the Web (MOTW) load failures
            if (ex is System.IO.FileLoadException || ex is NotSupportedException || 
                (ex.InnerException != null && (ex.InnerException is System.IO.FileLoadException || ex.InnerException is NotSupportedException)))
            {
                msg += "\n\nThis may be caused by Windows blocking the downloaded plugin files (Mark of the Web).\n" +
                       "Please close Notepad++, right-click the NppEdiPlugin ZIP or extracted DLLs in Explorer, select Properties, check 'Unblock', and try again.";
            }
            
            MessageBox.Show(msg, PluginName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
