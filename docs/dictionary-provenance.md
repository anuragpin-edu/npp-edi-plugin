# Dictionary Provenance

This document outlines the sources and provenance of the EDI dictionaries used within the NppEdiPlugin.

## Phase 1
- **Method:** Hand-authored dictionaries.
- **Scope:** Basic UN/EDIFACT support (MVP).
- **Source:** Created based on publicly available UN/EDIFACT documentation and specifications published by UNECE.
- **Licensing:** Hand-authored specifically for this plugin.

## Phase 2+
- **Method:** Imported JSON dictionary schemas.
- **Scope:** Comprehensive X12, EDIFACT, and VDA support.
- **Source:** The `vscode-edi-support` repository, specifically the `edi-parser` package schemas.
  - **Author/Attribution:** Copyright (c) 2017 Deric Lee.
  - **License:** MIT License. See `THIRD_PARTY_NOTICES.md` for the complete license text.

## Disclaimer
NppEdiPlugin is an independent open-source project and is not affiliated with or endorsed by ASC X12, UNECE, VDA, OpenText, Liaison Technologies, or the maintainers of `vscode-edi-support`.
