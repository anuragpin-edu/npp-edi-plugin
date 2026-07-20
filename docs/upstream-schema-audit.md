# Upstream Schema Audit

This document describes the schema JSON definitions imported from the `vscode-edi-support` repository to serve as the phase 2 data dictionary source.

## Upstream Details
- **Upstream repository URL:** https://github.com/hellooops/vscode-edi-support
- **Exact commit SHA:** 10d39d2495a1a8b5264bf35c311a1e07efa745e7
- **Import date:** 2026-07-15
- **License:** MIT (Copyright (c) 2017 Deric Lee). A copy of the license is provided in `THIRD_PARTY_NOTICES.md`. There are no separate licenses within the schema directories.

## Source Paths
- **X12:** `packages/edi-parser/src/schemas/x12/`
- **EDIFACT:** `packages/edi-parser/src/schemas/edifact/`
- **VDA:** `packages/edi-parser/src/schemas/vda/`

## X12 Releases
The following X12 releases are available:
- **002001**: 387 segments, 15 document types
- **002002**: 404 segments, 19 document types
- **002003**: 437 segments, 25 document types
- **00204**: 165 segments, 29 document types
- **00301**: 220 segments, 39 document types
- **00302**: 489 segments, 104 document types
- **00303**: 670 segments, 161 document types
- **00304**: 753 segments, 187 document types
- **00305**: 830 segments, 225 document types
- **00306**: 853 segments, 245 document types
- **00307**: 936 segments, 273 document types
- **00401**: 970 segments, 294 document types
- **00402**: 983 segments, 302 document types
- **00403**: 994 segments, 309 document types
- **00404**: 1001 segments, 314 document types
- **00405**: 1000 segments, 314 document types
- **00501**: 1004 segments, 318 document types
- **00502**: 1003 segments, 317 document types
- **00503**: 1003 segments, 317 document types
- **00504**: 1003 segments, 318 document types
- **00505**: 1005 segments, 318 document types
- **00601**: 1019 segments, 318 document types
- **00602**: 1019 segments, 318 document types
- **00603**: 1020 segments, 319 document types
- **00701**: 1020 segments, 0 document types

## EDIFACT Directories
Available EDIFACT versions: `021`, `3.0`, `4.1`, `D00A`, `D00B`, `D01A`, `D01B`, `D01C`, `D02A`, `D02B`, `D03A`, `D03B`, `D04A`, `D04B`, `D05A`, `D05B`, `D06A`, `D06B`, `D07A`, `D07B`, `D08A`, `D08B`, `D09A`, `D09B`, `D10A`, `D10B`, `D11A`, `D11B`, `D12A`, `D12B`, `D13A`, `D13B`, `D14A`, `D14B`, `D15A`, `D15B`, `D16A`, `D16B`, `D17A`, `D17B`, `D18A`, `D18B`, `D19A`, `D19B`, `D20A`, `D20B`, `D21A`, `D21B`, `D22A`, `D22B`, `D23A`, `D24A`, `D911`, `D912`, `D921`, `D932`, `D93A`, `D94A`, `D94B`, `D95A`, `D95B`, `D96A`, `D96B`, `D97A`, `D97B`, `D98A`, `D98B`, `D99A`, `D99B`, `S93A`

**D96A Specifics:**
- Segment count: 130
- Message type (document) count: 125
- Qualifier count: 211

## VDA Formats
The `vda` directory contains the following formats:
- `02`
- `03`

## Schema JSON Shape
The schema definitions for each release are typically split into two files: `{release}.json` and `{release}_versions.json`.

### `{release}.json`
This file defines segments and qualifiers. Top-level keys:
- **`Release`**: The release identifier (e.g., "00401", "D96A").
- **`Qualifiers`**: A map where keys are qualifier names (e.g., "Interchange ID Qualifier") and values are an object containing code-to-description mappings.
- **`Segments`**: A map where keys are segment IDs (e.g., "BGM", "N1") and values represent the segment's structure containing its elements and description.

### `{release}_versions.json`
This file outlines the document or transaction set structures. Top-level keys:
- **`Release`**: The release identifier.
- **`DocumentTypes`** (or message types): A map where keys denote the transaction/message identifier (e.g., "00401_850") and values define the expected segments, loops, and their hierarchical structure.
  - Features nested structures (like `TransactionSet` arrays containing segments or `Loop` objects).

## Qualifiers and Composite Elements
- **Qualifiers:** Elements inside a segment may specify a `QualifierRef`. This string references a key in the top-level `Qualifiers` map, which details the valid codes for that element.
- **Composite Elements:** Certain elements (especially in EDIFACT) can be composites containing multiple sub-elements. These are represented using a `Components` array within the element's JSON definition.

## Known Gaps or Limitations
- The `00701` X12 release contains segments but zero defined document types in its `_versions.json` file.
- The JSON schemas are tailored to the parsing engine of `vscode-edi-support` and may require mapping logic to translate effectively to our Notepad++ plugin's internal models.
