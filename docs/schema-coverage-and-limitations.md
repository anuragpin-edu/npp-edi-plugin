# EDI Schema Coverage & Limitations

This document outlines the EDI schema versions supported by the `NppEdiPlugin` as of the Phase 3 integration, along with known limitations and performance considerations.

## Coverage Scope

The upstream repository `hellooops/vscode-edi-support` contains data definitions for **25 X12 releases** (002001-00701) and **61 EDIFACT releases** (021-S93A, D00A-D24A). Our python importer tool (`import_schemas.py`) can generate dictionaries for all of these upon request.

However, due to the massive size of these JSON dictionaries, only the following core, highly-utilized releases are **committed** with `NppEdiPlugin` and actively shipped to end users:

*   **X12 00401** (Size: ~46MB)
    *   Segments: 970
    *   Elements: 7,850
    *   Codes/Qualifiers: 688,581
    *   Messages/Transactions: 294
*   **X12 00501** (Size: ~54MB)
    *   Segments: 1,004
    *   Elements: 8,186
    *   Codes/Qualifiers: 826,030
    *   Messages/Transactions: 318
*   **EDIFACT D96A** (Size: ~3.9MB)
    *   Segments: 130
    *   Elements: 472
    *   Codes/Qualifiers: 3,889
    *   Messages/Transactions: 125

*(Note: The `messages` structural definitions are imported from the upstream `_versions.json`, but currently only serve as a reference; the runtime plugin does **not** enforce segment structure/hierarchy using these definitions yet.)*

### Fallback Handling

Integration tests guarantee that parsing behaves safely around uncommitted dictionaries:
1. **HIPAA Variations:** The parser automatically normalizes strings like `005010X222A1` down to `00501` to safely load the shipped dictionary.
2. **Missing Dictionaries:** If an unsupported document is opened (e.g. `X12 00701` or `EDIFACT D18A`), the parser handles the document gracefully—delimiters and structural envelopes parse cleanly, but semantic enrichment (segment labels and element descriptions) is bypassed.

## Known Limitations

1. **Memory Limitations**: 
   The generated `.json` files are incredibly dense. Loading `x12_00501.json` loads hundreds of thousands of individual qualifier descriptions into the C# `JsonEdiDictionary` mapping objects. Memory limits dictate we restrict the shipped dictionaries.
2. **Missing Qualifiers**:
   If an unknown qualifier code is used within a supported dictionary, the specific `ValueDescription` will silently remain null without crashing the parser.
3. **No Structural Validation**:
   Dictionaries provide semantic enrichment but the parser currently does *not* enforce segment ordering, required/optional presence, or segment looping rules beyond standard envelope verification.

## Performance Considerations

- **Lazy Loading**: Dictionaries are loaded only on-demand when an EDI document of a specific version is opened and parsed.
- **Cache Persistence**: Once a dictionary is loaded into memory, it is cached statically for the duration of the Notepad++ session to eliminate repetitive disk I/O and deserialization overhead.
- **Regex-Free Core Parser**: The actual tokenization and parsing of EDI text (locating segments, elements, and components via delimiters) is completely regex-free and highly optimized to minimize GC pressure.
