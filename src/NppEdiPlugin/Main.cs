using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;
using NppEdiPlugin.Forms;
using Edi.Core.Formatting;
using Edi.Core.Model;
using Edi.Core.Parsing;
using Edi.Edifact.Formatting;
using Edi.Edifact.Parsing;
using Edi.Dictionaries;

namespace Kbg.NppPluginNET
{
    class Main
    {
        internal const string PluginName = "NppEdiPlugin";
        
        static string iniFilePath = null;
        static EdiTreeForm frmMyDlg = null;
        static int idMyDlg = -1;
        static Bitmap tbBmp = new Bitmap(16, 16); 
        static Bitmap tbBmp_tbTab = new Bitmap(16, 16);
        static Icon tbIcon = null;
        
        // Dummy properties to satisfy NppCSharpPluginPack Utils dependencies
        public static string PluginConfigDirectory { get; set; }
        public static bool isShuttingDown = false;
        public static Form selectionRememberingForm = null;
        public static int IdCloseHtmlTag = -1;
        public static void RestyleEverything() { }
        internal static void SetToolBarIcons() { SetToolBarIcon(); }

        static ScintillaGateway scintilla = new ScintillaGateway(PluginBase.GetCurrentScintilla());
        static NotepadPPGateway notepad = new NotepadPPGateway();

        // Generic dispatcher setup — add new parsers/formatters here as standards are implemented.
        static EdiParserDispatcher parserDispatcher = new EdiParserDispatcher(new IEdiParser[] { new EdifactParser() });
        static EdiFormatterDispatcher formatterDispatcher = new EdiFormatterDispatcher(new IEdiFormatter[] { new EdifactFormatter() });
        static JsonEdiDictionary dictionary = new JsonEdiDictionary();

        public static void OnNotification(ScNotification notification)
        {  
            // Can be used to hook into Notepad++ events
        }

        internal static void CommandMenuInit()
        {
            PluginBase.SetCommand(0, "Parse EDI", ParseEdi, new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(1, "Prettify EDI", PrettifyEdi, new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(2, "Minify EDI", MinifyEdi, new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(3, "Toggle EDI Tree Panel", ToggleTreePanel, new ShortcutKey(false, false, false, Keys.None));
            idMyDlg = 3; // Used for dockable panel tracking
            PluginBase.SetCommand(4, "---", null);
            PluginBase.SetCommand(5, "About NppEdiPlugin", About);
        }

        internal static void SetToolBarIcon()
        {
            // Register toolbar icon if needed
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = tbBmp.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[idMyDlg]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }

        internal static void PluginCleanUp()
        {
            // Clean up
        }

        /// <summary>
        /// Reads the active Scintilla buffer and parses it using the generic parser dispatcher.
        /// Auto-detects the EDI standard. Shows results in the dockable tree panel.
        /// </summary>
        internal static void ParseEdi()
        {
            if (frmMyDlg == null || !frmMyDlg.Visible)
            {
                ToggleTreePanel();
            }

            string text = GetEditorText();
            var document = parserDispatcher.Parse(text, dictionary);
            
            if (document.Standard == EdiStandard.Unknown)
            {
                MessageBox.Show(
                    "Could not detect a supported EDI standard in the current document.\n\n" +
                    "Supported standards: EDIFACT",
                    PluginName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (frmMyDlg != null)
            {
                frmMyDlg.RenderDocument(document);
            }
        }

        /// <summary>
        /// Reads the active Scintilla buffer, prettifies it using the formatter dispatcher,
        /// and replaces the buffer contents on success.
        /// </summary>
        internal static void PrettifyEdi()
        {
            string text = GetEditorText();
            var result = formatterDispatcher.Prettify(text);

            if (!result.IsSuccess)
            {
                MessageBox.Show(
                    result.ErrorMessage,
                    PluginName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ReplaceEditorText(result.FormattedText);
        }

        /// <summary>
        /// Reads the active Scintilla buffer, minifies it using the formatter dispatcher,
        /// and replaces the buffer contents on success.
        /// </summary>
        internal static void MinifyEdi()
        {
            string text = GetEditorText();
            var result = formatterDispatcher.Minify(text);

            if (!result.IsSuccess)
            {
                MessageBox.Show(
                    result.ErrorMessage,
                    PluginName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ReplaceEditorText(result.FormattedText);
        }

        internal static void ToggleTreePanel()
        {
            if (frmMyDlg == null)
            {
                frmMyDlg = new EdiTreeForm(scintilla);

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = frmMyDlg.Handle;
                _nppTbData.pszName = "EDI Inspector";
                _nppTbData.dlgID = idMyDlg;
                // User requested LEFT side docking
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_LEFT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint)tbBmp_tbTab.GetHicon();
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
                Marshal.FreeHGlobal(_ptrNppTbData);
            }
            else
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_DMMSHOW, 0, frmMyDlg.Handle);
            }
        }

        internal static void About()
        {
            MessageBox.Show(
                "NppEdiPlugin — EDI Inspector for Notepad++\n" +
                "Supports: EDIFACT (X12 and VDA planned)\n\n" +
                "Phase 2 — Generic dispatchers",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // ── Helper methods ──────────────────────────────────────────────────

        /// <summary>
        /// Reads the full text from the active Scintilla editor.
        /// </summary>
        private static string GetEditorText()
        {
            int length = scintilla.GetTextLength();
            return scintilla.GetText(length + 1);
        }

        /// <summary>
        /// Replaces the entire text in the active Scintilla editor.
        /// </summary>
        private static void ReplaceEditorText(string newText)
        {
            scintilla.SelectAll();
            scintilla.ReplaceSel(newText);
        }
    }
}
