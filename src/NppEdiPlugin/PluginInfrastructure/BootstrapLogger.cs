// Bootstrap-only logger. No references to Main, Edi.Core, or any Edi.* type.
// Must never throw under any circumstances.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Kbg.NppPluginNET
{
    internal static class BootstrapLogger
    {
        private static readonly string _logPath;
        private static readonly object _lock = new object();

        static BootstrapLogger()
        {
            try
            {
                string temp = Environment.GetEnvironmentVariable("TEMP")
                    ?? Environment.GetEnvironmentVariable("TMP")
                    ?? Path.GetTempPath();
                _logPath = Path.Combine(temp, "NppEdiPlugin-bootstrap.log");
                // Truncate on first use per session
                File.WriteAllText(_logPath, string.Empty, Encoding.UTF8);
            }
            catch
            {
                _logPath = null;
            }
        }

        /// <summary>Returns the path where the bootstrap log is written.</summary>
        internal static string LogPath => _logPath;

        /// <summary>Write a plain message line.</summary>
        internal static void Log(string message)
        {
            if (_logPath == null) return;
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logPath,
                        $"[{DateTime.UtcNow:HH:mm:ss.fff}Z] {message}{Environment.NewLine}",
                        Encoding.UTF8);
                }
            }
            catch { /* never throw */ }
        }

        /// <summary>Write process / runtime environment details.</summary>
        internal static void LogEnvironment()
        {
            try
            {
                var asm = typeof(BootstrapLogger).Assembly;
                Log($"  ProcessBitness : {(IntPtr.Size == 8 ? "64-bit" : "32-bit")}");
                Log($"  CLR Version    : {Environment.Version}");
                Log($"  AppDomain.BaseDirectory : {AppDomain.CurrentDomain.BaseDirectory}");
                Log($"  Assembly.Location       : {asm.Location}");
                Log($"  CurrentDirectory        : {Directory.GetCurrentDirectory()}");
                Log($"  PluginDirectory (derived): {Path.GetDirectoryName(asm.Location)}");
            }
            catch (Exception ex) { Log($"  [LogEnvironment error] {ex.Message}"); }
        }

        /// <summary>Write all currently-loaded assembly names and locations.</summary>
        internal static void LogLoadedAssemblies()
        {
            try
            {
                Log("  Loaded assemblies:");
                var loaded = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var a in loaded)
                {
                    string loc;
                    try { loc = a.Location ?? "(dynamic)"; }
                    catch { loc = "(error)"; }
                    Log($"    {a.FullName} => {loc}");
                }
            }
            catch (Exception ex) { Log($"  [LogLoadedAssemblies error] {ex.Message}"); }
        }

        /// <summary>Log an exception, walking the entire InnerException chain and
        /// including FusionLog where available.</summary>
        internal static void LogException(string context, Exception ex)
        {
            if (ex == null) return;
            try
            {
                Log($"  [EXCEPTION in {context}] {ex.GetType().FullName}: {ex.Message}");
                Log(ex.ToString());

                Exception inner = ex;
                while (inner != null)
                {
                    TryLogFusionLog(inner);
                    inner = inner.InnerException;
                }
            }
            catch { /* never throw */ }
        }

        private static void TryLogFusionLog(Exception ex)
        {
            try
            {
                // Check for FusionLog property via reflection to avoid compile-time ref to FileNotFoundException
                var prop = ex.GetType().GetProperty("FusionLog",
                    BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return;
                var fusionLog = prop.GetValue(ex) as string;
                if (!string.IsNullOrEmpty(fusionLog))
                    Log($"  [FusionLog] {fusionLog}");
            }
            catch { /* never throw */ }
        }
    }
}
