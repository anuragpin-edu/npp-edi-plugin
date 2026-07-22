# Manual Smoke Test Results

## Environment
*   **Windows Version:** Windows 11 Home 25H2 (OS Build: 26200.8655)
*   **Notepad++ Version:** 8.9.7 (64-bit)
*   **Admin Mode:** OFF
*   **Local Conf Mode:** OFF
*   **Multi-instance Mode:** monoInst
*   **Installation Type:** Standard
*   **Plugin Path:** `C:\Program Files\Notepad++\plugins\NppEdiPlugin\`
*   **Bundled plugins only:** mimeTools, NppConverter, NppExport

## Package History

| Package | NppEdiPlugin.dll SHA-256 | ZIP SHA-256 | LOD-01 Result |
| :--- | :--- | :--- | :--- |
| Original (main `29758724385`) | `7587e7af1a76aa71f0f661eaf2ac3ba44bc7f68f3ffa64fc6633de12581123cc` | `c40e99aa7478e5331b562f07000adec142cf7e49422bcd63b1fdcdb85dff1cee` | **FAIL** — DLL missing dependency closure |
| Multi-DLL PR#3 (`29870795249`) | `cc5d0f864e77dc7059e0e84ca8086c414ef8c3f19f8d8cdd60409aa109b89f3f` | `80885c3dcaf8663d99c17f399441d1ae5836ad5d31ff12ffabdac75c4dce21a8` | **FAIL** — "not compatible" — probable exception in setInfo() / eager static init |
| Diagnostic PR#3 (fix/plugin-load-and-runtime-package) | _TBD after CI_ | _TBD_ | Pending install |

## Results Table

*(Status: `Not executed`, `Pass`, `Fail`, `Blocked`, `Verified only through automated tests`)*

| Test ID | Standard | Version | Test Action | Expected Result | Actual Result | Status | Notes | Screenshot |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **LOD-01** | N/A | N/A | Install original package (single DLL), start Notepad++ | Plugin loads, menu appears | "Failed to load" error | **FAIL** | Missing dependency DLLs |
| **LOD-01b** | N/A | N/A | Install multi-DLL PR#3 package, start Notepad++ | Plugin loads, menu appears | "not compatible" error | **FAIL** | Exception likely in setInfo() eager static init |
| **LOD-02** | N/A | N/A | Check Plugins menu | NppEdiPlugin appears in Plugins menu. | | **BLOCKED** — LOD-01 unresolved | | |
| **LOD-03** | N/A | N/A | Open EDI Tree panel | The EDI Tree panel opens/closes correctly. | | **BLOCKED** | | |
| **LOD-04** | N/A | N/A | Restart Notepad++ | No duplicate menu commands or panels appear. | | **BLOCKED** | | |
| **NAV-01** | EDIFACT | D96A | Select segment in tree | Highlights the source segment. | | Not executed | | |
| **NAV-02** | X12 | 00501 | Select off-screen segment | Scrolls it into view. | | Not executed | | |
| **NAV-03** | X12 | 00501 | Repeated navigation | Does not throw an exception. | | Not executed | | |
| **NAV-04** | X12 | 00501 | Open second document | Updates the tree correctly. | | Not executed | | |
| **NAV-05** | EDIFACT | D96A | EDIFACT & X12 open | Switch tabs updates tree correctly. | | Not executed | | |
| **NAV-06** | X12 | 00501 | Close active document | Does not leave invalid selection behavior. | | Not executed | | |
| **NAV-07** | N/A | N/A | Open non-EDI text file | Tree clears or shows empty/unsupported. | | Not executed | | |
| **EDF-01** | EDIFACT | D96A | Parse `edifact_d96a_orders_with_una.edi` | Correct standard/version, dict D96A selected. | | Not executed | | |
| **EDF-02** | EDIFACT | D96A | Check semantic labels (EDF-01) | BGM labelled "Beginning of Message", elements appear. | | Not executed | | |
| **EDF-03** | EDIFACT | D96A | Prettify (EDF-01) | UNA preserved, one segment per line. | | Not executed | | |
| **EDF-04** | EDIFACT | D96A | Minify (EDF-01) | Formatting removed, values unchanged, re-parsing works. | | Not executed | | |
| **EDF-05** | EDIFACT | D96A | Parse `edifact_d96a_orders_no_una.edi` | Parses successfully using default delimiters. | | Not executed | | |
| **EDF-06** | EDIFACT | D96A | Parse `edifact_d96a_escaped.edi` | Escaped delimiters `?` parsed correctly. | | Not executed | | |
| **EDF-07** | EDIFACT | D18A | Parse `edifact_d18a_unsupported.edi` | Parses structurally, shows raw version, lacks semantic enrichment. | | Not executed | | |
| **EDF-08** | EDIFACT | D96A | Parse `edifact_malformed_envelope.edi` | Shows validation issues for malformed UNB/UNZ. | | Not executed | | |
| **X12-01** | X12 | 004010 | Parse `x12_004010_850.edi` | Correct standard/version, dict 00401 selected. | | Not executed | | |
| **X12-02** | X12 | 005010 | Parse `x12_005010_850.edi` | Correct standard/version, dict 00501 selected. | | Not executed | | |
| **X12-03** | X12 | 005010 | Check semantic labels (X12-02) | BEG labelled "Beginning Segment...", BEG02 labelled. | | Not executed | | |
| **X12-04** | X12 | 005010 | Prettify (X12-02) | CRLF/LF line endings, fixed-width ISA preserved. | | Not executed | | |
| **X12-05** | X12 | 00501 | Parse `x12_005010X222A1_837.edi` | Uses 00501 dictionary, preserves raw version `005010X222A1`. | | Not executed | | |
| **X12-06** | X12 | 00501 | Parse `x12_custom_delimiters.edi` | Custom delimiters (e.g. `|`, `~`) parsed correctly. ISA length preserved. | | Not executed | | |
| **X12-07** | X12 | 00501 | Parse `x12_crlf_formatted.edi` | CRLF formatted segments parsed successfully. | | Not executed | | |
| **X12-08** | X12 | 00200 | Parse `x12_unsupported.edi` | Parses structurally, shows raw version, lacks semantic enrichment. | | Not executed | | |
| **X12-09** | X12 | 00501 | Parse `x12_malformed_isa.edi` | Validation issues for invalid ISA length/format. | | Not executed | | |
| **X12-10** | X12 | 00501 | Parse `x12_count_mismatch.edi` | Validation issues for ST/SE count and ISA/IEA mismatch. | | Not executed | | |
| **DEP-01** | X12 | 00501 | Rename deployed `Data` directory & Parse | Structural parsing works, labels disappear gracefully, no crash. | | Not executed | | |
| **DEP-02** | X12 | 00501 | Restore `Data` directory & Restart | Labels return correctly. | | Not executed | | |
| **LRG-01** | X12 | 00501 | Parse ~10,000 segment X12 | Notepad++ remains responsive, parsing completes, memory growth noted. | | Not executed | | |
| **LRG-02** | EDIFACT | D96A | Parse ~30,000 segment EDIFACT | Notepad++ remains responsive, parsing completes, memory growth noted. | | Not executed | | |

## Defect Tracking
*List any defects found below, matching the Test ID above.*
