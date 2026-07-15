# EDI Schema Coverage & Limitations

This document outlines the EDI schema versions supported by the `NppEdiPlugin` as of the Phase 1/2 integration, along with known limitations and performance considerations.

## Coverage

The plugin ships with an extensive set of dictionaries generated from upstream `vscode-edi-support` repositories.

### X12 Supported Releases
We currently support 25 X12 schema releases:
- From legacy versions: `002001`, `002002`, `002003`, `00204`, `00301`-`00307`
- Standard active versions: `00401` through `00405`, `00501` through `00505`
- Modern versions: `00601`-`00603`, `00701`

**HIPAA Extensions:** The parser includes robust version fallback handling for HIPAA extensions (e.g., `005010X222A1` automatically uses the `00501` dictionary).

### EDIFACT Supported Releases
We currently support 61 EDIFACT schema releases:
- Early versions: `021`, `3.0`, `4.1`
- Directory releases spanning decades: `D93A`-`D99B`, `D00A`-`D24A` (as well as variants like `D911`, `D921`, `S93A`).

## Known Limitations

1. **Memory & Size Limitations**: 
   The generated `.json` files are highly detailed and can be multiple megabytes each (e.g. `x12_00501.json` is ~56MB). Loading many different dictionaries in a single Notepad++ session can consume substantial memory.
2. **Missing Qualifiers**:
   If an unknown qualifier code is used (e.g., in a message BGM segment), the element will be labeled with its generic segment position name, but the specific `ValueDescription` will silently remain null.
3. **No Structural Validation (Yet)**:
   Dictionaries provide semantic enrichment (labels and descriptions) but the parser currently does *not* enforce segment ordering, required/optional element presence, or segment looping rules beyond standard envelope checking.

## Performance Considerations

- **Lazy Loading**: Dictionaries are loaded only on-demand when an EDI document of a specific version is opened and parsed.
- **Cache Persistence**: Once a dictionary is loaded into memory, it is cached for the duration of the Notepad++ session to eliminate repetitive disk I/O and deserialization overhead.
- **Regex-Free Core Parser**: The actual tokenization and parsing of EDI text (locating segments, elements, and components via delimiters) is completely regex-free and highly optimized to minimize GC pressure.
