using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;
using NppEdiPlugin.Forms;
using Edi.Core.Parsing;
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

        // Parser setup
        static EdiParserDispatcher parserDispatcher = new EdiParserDispatcher(new[] { new EdifactParser() });
        static JsonEdiDictionary dictionary = new JsonEdiDictionary();

        public static void OnNotification(ScNotification notification)
        {  
            // Can be used to hook into Notepad++ events
        }

        internal static void CommandMenuInit()
        {
            PluginBase.SetCommand(0, "Parse EDIFACT", ParseEdi, new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(1, "Toggle EDI Tree Panel", ToggleTreePanel, new ShortcutKey(false, false, false, Keys.None));
            idMyDlg = 1; // Used for dockable panel tracking
            PluginBase.SetCommand(2, "---", null);
            PluginBase.SetCommand(3, "About NppEdiPlugin", About);
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

        internal static void ParseEdi()
        {
            if (frmMyDlg == null || !frmMyDlg.Visible)
            {
                ToggleTreePanel();
            }

            int length = scintilla.GetTextLength();
            string text = scintilla.GetText(length + 1);

            var document = parserDispatcher.Parse(text, dictionary);
            
            if (frmMyDlg != null)
            {
                frmMyDlg.RenderDocument(document);
            }
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
            MessageBox.Show("NppEdiPlugin - EDI Inspector for Notepad++\nPhase 2 Preview", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
