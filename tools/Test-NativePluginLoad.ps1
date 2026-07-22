<#
.SYNOPSIS
  Minimal native-load test host that mimics early Notepad++ DLL load sequence.
  Calls LoadLibraryEx, resolves all required exports, then invokes isUnicode()
  and getName(). Does NOT call setInfo() because Main currently emits window
  messages to a real Notepad++ HWND.

.OUTPUTS
  Exits with code 0 on success, 1 on any failure.
#>
param(
    [Parameter(Mandatory=$true)]
    [string]$PluginDll
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Output "=== NppEdiPlugin Native Load Test ==="
Write-Output "  DLL: $PluginDll"

if (-not (Test-Path $PluginDll)) {
    Write-Error "DLL not found: $PluginDll"
    exit 1
}

# Resolve absolute path so LoadLibraryEx succeeds
$PluginDll = (Resolve-Path $PluginDll).Path

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
using System.Text;

public static class NativeLoader
{
    public const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeLibrary(IntPtr hModule);

    // Delegate types matching Notepad++ calling convention (Cdecl)
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public delegate bool IsUnicodeDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr GetNameDelegate();
}
"@ -Language CSharp

Write-Output ""
Write-Output "--- LoadLibraryEx ---"
$hModule = [NativeLoader]::LoadLibraryEx($PluginDll, [IntPtr]::Zero, [NativeLoader]::LOAD_WITH_ALTERED_SEARCH_PATH)
if ($hModule -eq [IntPtr]::Zero) {
    $err = [Runtime.InteropServices.Marshal]::GetLastWin32Error()
    Write-Error "LoadLibraryEx failed with Win32 error: $err"
    exit 1
}
Write-Output "  LoadLibraryEx => handle 0x$($hModule.ToString('X'))"

$allOk = $true
try {
    $requiredExports = @("isUnicode","setInfo","getFuncsArray","messageProc","getName","beNotified")
    Write-Output ""
    Write-Output "--- GetProcAddress for required exports ---"
    $exportPtrs = @{}
    foreach ($name in $requiredExports) {
        $ptr = [NativeLoader]::GetProcAddress($hModule, $name)
        if ($ptr -eq [IntPtr]::Zero) {
            Write-Error "GetProcAddress('$name') returned NULL"
            $allOk = $false
        } else {
            Write-Output "  $name => 0x$($ptr.ToString('X'))"
            $exportPtrs[$name] = $ptr
        }
    }

    if (-not $allOk) {
        Write-Error "One or more exports are missing."
        exit 1
    }

    Write-Output ""
    Write-Output "--- Calling isUnicode() ---"
    $isUnicodeFn = [Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer(
        $exportPtrs["isUnicode"],
        [NativeLoader+IsUnicodeDelegate]
    )
    $unicodeResult = $isUnicodeFn.Invoke()
    if (-not $unicodeResult) {
        Write-Error "isUnicode() returned false — expected true"
        $allOk = $false
    } else {
        Write-Output "  isUnicode() => $unicodeResult  [OK]"
    }

    Write-Output ""
    Write-Output "--- Calling getName() ---"
    $getNameFn = [Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer(
        $exportPtrs["getName"],
        [NativeLoader+GetNameDelegate]
    )
    $namePtr = $getNameFn.Invoke()
    if ($namePtr -eq [IntPtr]::Zero) {
        Write-Error "getName() returned NULL"
        $allOk = $false
    } else {
        $pluginName = [Runtime.InteropServices.Marshal]::PtrToStringUni($namePtr)
        Write-Output "  getName() => '$pluginName'  [OK]"
        if ($pluginName -ne "NppEdiPlugin") {
            Write-Warning "Plugin name is '$pluginName', expected 'NppEdiPlugin'"
        }
    }
}
finally {
    [NativeLoader]::FreeLibrary($hModule) | Out-Null
    Write-Output ""
    Write-Output "--- FreeLibrary called ---"
}

if ($allOk) {
    Write-Output ""
    Write-Output "=== Native load test PASSED ==="
    exit 0
} else {
    Write-Error "=== Native load test FAILED ==="
    exit 1
}
