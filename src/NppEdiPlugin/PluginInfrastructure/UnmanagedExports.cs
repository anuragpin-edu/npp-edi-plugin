// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
// Modified: added bootstrap-stage logging, AssemblyResolve handler, lazy Main init.
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Kbg.NppPluginNET.PluginInfrastructure;
using RGiesecke.DllExport;

namespace Kbg.NppPluginNET
{
    class UnmanagedExports
    {
        // ─── Assembly-resolve state ───────────────────────────────────────────
        private static volatile bool _resolveRegistered = false;
        private static volatile bool _resolveInProgress = false;

        /// <summary>
        /// Registers an AppDomain.AssemblyResolve handler that probes for
        /// managed assemblies beside the plugin DLL.  This MUST be called
        /// before any Edi.* or System.Text.Json type is referenced.
        /// </summary>
        private static void EnsureAssemblyResolveRegistered()
        {
            if (_resolveRegistered) return;
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                _resolveRegistered = true;
                BootstrapLogger.Log("  AssemblyResolve handler registered.");
            }
            catch (Exception ex)
            {
                BootstrapLogger.Log($"  [WARN] Could not register AssemblyResolve: {ex.Message}");
            }
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Prevent recursive calls
            if (_resolveInProgress) return null;
            _resolveInProgress = true;
            try
            {
                // Derive plugin directory from this assembly's location (never AppDomain.BaseDirectory)
                string pluginDir = Path.GetDirectoryName(
                    typeof(UnmanagedExports).Assembly.Location ?? string.Empty) ?? string.Empty;

                // Extract simple assembly name (strip version/culture/token)
                string fullName = args.Name ?? string.Empty;
                string simpleName = fullName.Contains(",")
                    ? fullName.Substring(0, fullName.IndexOf(','))
                    : fullName;

                // Do not hijack mscorlib / System / WindowsBase etc.
                if (simpleName.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase)
                    || simpleName.StartsWith("System.Windows.Forms", StringComparison.OrdinalIgnoreCase)
                    || simpleName.StartsWith("System.Drawing", StringComparison.OrdinalIgnoreCase)
                    || simpleName.StartsWith("PresentationFramework", StringComparison.OrdinalIgnoreCase)
                    || simpleName.StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                string candidate = Path.Combine(pluginDir, simpleName + ".dll");
                BootstrapLogger.Log($"  [AssemblyResolve] Request='{simpleName}' Probe='{candidate}'");

                if (File.Exists(candidate))
                {
                    BootstrapLogger.Log($"  [AssemblyResolve] Loading from '{candidate}'");
                    return Assembly.LoadFrom(candidate);
                }

                BootstrapLogger.Log($"  [AssemblyResolve] Not found at probe path.");
                return null;
            }
            catch (Exception ex)
            {
                BootstrapLogger.Log($"  [AssemblyResolve ERROR] {ex.Message}");
                return null;
            }
            finally
            {
                _resolveInProgress = false;
            }
        }

        // ─── Exports ──────────────────────────────────────────────────────────

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static bool isUnicode()
        {
            try
            {
                BootstrapLogger.Log(">>> isUnicode() entered");
                BootstrapLogger.Log("<<< isUnicode() returning true");
                return true;
            }
            catch (Exception ex)
            {
                BootstrapLogger.LogException("isUnicode", ex);
                throw;
            }
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void setInfo(NppData notepadPlusData)
        {
            BootstrapLogger.Log(">>> setInfo() entered");
            try
            {
                BootstrapLogger.LogEnvironment();

                // Stage 1 — register resolver BEFORE any Edi.* types are touched
                BootstrapLogger.Log("  [Stage 1] Registering AssemblyResolve handler");
                EnsureAssemblyResolveRegistered();

                // Stage 2 — assign nppData (no Edi.* reference)
                BootstrapLogger.Log("  [Stage 2] Assigning PluginBase.nppData");
                PluginBase.nppData = notepadPlusData;

                // Stage 3 — call CommandMenuInit (references Main but NOT Edi.* at class-init time)
                BootstrapLogger.Log("  [Stage 3] Before Main.CommandMenuInit()");
                Main.CommandMenuInit();
                BootstrapLogger.Log("  [Stage 4] After Main.CommandMenuInit() — success");

                BootstrapLogger.LogLoadedAssemblies();
                BootstrapLogger.Log("<<< setInfo() returning normally");
            }
            catch (Exception ex)
            {
                BootstrapLogger.LogException("setInfo", ex);
                BootstrapLogger.Log("<<< setInfo() rethrowing after logging");
                throw;
            }
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getFuncsArray(ref int nbF)
        {
            try
            {
                BootstrapLogger.Log(">>> getFuncsArray() entered");
                nbF = PluginBase._funcItems.Items.Count;
                BootstrapLogger.Log($"<<< getFuncsArray() returning count={nbF}");
                return PluginBase._funcItems.NativePointer;
            }
            catch (Exception ex)
            {
                BootstrapLogger.LogException("getFuncsArray", ex);
                throw;
            }
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                BootstrapLogger.Log($">>> messageProc(msg={Message}) entered");
                BootstrapLogger.Log("<<< messageProc() returning 1");
                return 1;
            }
            catch (Exception ex)
            {
                BootstrapLogger.LogException("messageProc", ex);
                throw;
            }
        }

        private static IntPtr _ptrPluginName = IntPtr.Zero;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getName()
        {
            try
            {
                BootstrapLogger.Log(">>> getName() entered");
                if (_ptrPluginName == IntPtr.Zero)
                    _ptrPluginName = Marshal.StringToHGlobalUni(Main.PluginName);
                BootstrapLogger.Log($"<<< getName() returning ptr for '{Main.PluginName}'");
                return _ptrPluginName;
            }
            catch (Exception ex)
            {
                BootstrapLogger.LogException("getName", ex);
                throw;
            }
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void beNotified(IntPtr notifyCode)
        {
            try
            {
                BootstrapLogger.Log($">>> beNotified(ptr={notifyCode}) entered");
                ScNotification notification = (ScNotification)Marshal.PtrToStructure(notifyCode, typeof(ScNotification));
                if (notification.Header.Code == (uint)NppMsg.NPPN_TBMODIFICATION)
                {
                    PluginBase._funcItems.RefreshItems();
                    Main.SetToolBarIcons();
                }
                else if (notification.Header.Code == (uint)NppMsg.NPPN_SHUTDOWN)
                {
                    Main.PluginCleanUp();
                    if (_ptrPluginName != IntPtr.Zero)
                        Marshal.FreeHGlobal(_ptrPluginName);
                }
                else
                {
                    Main.OnNotification(notification);
                }
                BootstrapLogger.Log($"<<< beNotified() returning normally");
            }
            catch (Exception ex)
            {
                BootstrapLogger.LogException("beNotified", ex);
                throw;
            }
        }
    }
}
