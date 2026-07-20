# NppEdiPlugin: Manual Smoke Test Guide

This document provides exact steps to install the actual compiled `NppEdiPlugin` inside a real 64-bit Notepad++ instance on Windows and execute a manual smoke test.

## Prerequisites

- Windows 10/11
- A **64-bit** installation of Notepad++ (v8.4 or newer recommended).
- It is highly recommended to test in a **clean portable 64-bit Notepad++ installation** or a fully cleaned plugin directory to isolate variables.
- Download the test package ZIP. The package is the exact artifact from the `main` branch GitHub Actions CI run.

## Test Package Verification

Ensure you are testing the correct artifact:
- **CI Run ID:** 29758724385
- **NppEdiPlugin-x64.zip SHA-256:** `c40e99aa7478e5331b562f07000adec142cf7e49422bcd63b1fdcdb85dff1cee`
- **NppEdiPlugin.dll SHA-256:** `7587e7af1a76aa71f0f661eaf2ac3ba44bc7f68f3ffa64fc6633de12581123cc`

## Installation Steps

1. **Close Notepad++** completely.
2. **Confirm Notepad++ is 64-bit.** Open Notepad++, go to `?` -> `About Notepad++`, and verify it says `64-bit x64`. Close Notepad++ again.
3. Locate your Notepad++ plugins directory:
   - For a standard installation: `%PROGRAMFILES%\Notepad++\plugins\`
   - For a portable installation: `<Notepad++ Portable Folder>\plugins\`
4. Inside the `plugins` directory, create a new folder named exactly **`NppEdiPlugin`**.
5. Extract the contents of `NppEdiPlugin-x64.zip` and copy them into the `NppEdiPlugin` folder. The final layout must be:
   ```
   Notepad++\
     plugins\
       NppEdiPlugin\
         NppEdiPlugin.dll
         Data\
           edifact_D96A.json
           x12_00401.json
           x12_00501.json
           import-manifest.json
   ```
   *(Note: Do NOT copy files directly into the root Notepad++ directory or directly into the `plugins` folder without the `NppEdiPlugin` wrapper folder.)*
6. **Start Notepad++**.
7. Confirm the plugin loaded successfully by looking for **NppEdiPlugin** in the `Plugins` menu at the top.

## Uninstall / Rollback

To uninstall the plugin:
1. Close Notepad++.
2. Delete the `%PROGRAMFILES%\Notepad++\plugins\NppEdiPlugin` directory completely.
3. Restart Notepad++.

## Troubleshooting Plugin-Loading Failures

If the plugin does not appear in the menu or an error dialog is shown at startup:

1. **Verify Architecture:** Double-check that you are running 64-bit Notepad++. A 64-bit DLL will fail to load in 32-bit Notepad++.
2. **Check Windows Block:** Right-click `NppEdiPlugin.dll` -> `Properties`. If there is an "Unblock" checkbox at the bottom (saying the file came from another computer), check it and click Apply.
3. **Capture Debug Info:** In Notepad++, go to `?` -> `Debug Info...`, click `Copy debug info into clipboard`, and save it.
4. **Capture Startup Error Text:** If an error dialog appears, take a screenshot or write down the exact text.
5. **Verify Directory Structure:** Ensure the DLL is exactly at `plugins\NppEdiPlugin\NppEdiPlugin.dll`, not nested in an extra folder (e.g., `plugins\NppEdiPlugin\NppEdiPlugin-x64\NppEdiPlugin.dll`).
6. **Check Event Viewer:** Open Windows Event Viewer (`eventvwr`), check `Windows Logs` -> `Application` for any `.NET Runtime` or `Application Error` entries matching Notepad++ around the time of the crash.

## Executing the Test

1. Copy the test fixtures from `tests/ManualSamples/` to your Windows machine.
2. Execute the steps in `docs/manual-smoke-test-results.md` and record your results.
