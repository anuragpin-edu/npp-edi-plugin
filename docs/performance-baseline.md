# Performance Baseline (Phase 3)

This document presents an informal performance baseline generated on an Apple Silicon machine (arm64, .NET 8 Release build) during Phase 3 stabilization. It measures the performance characteristics of parsing large EDI payloads with and without semantic enrichment dictionaries. Note that these values are taken from a single test run and should be treated as approximate, non-statistical observations rather than benchmark-grade precision metrics.

## Hardware/Environment
*   Architecture: arm64
*   Runtime: .NET 8.0.29 (Release Mode)

## X12 Baseline (10,000 Segments)
*   **Input file size:** ~359 KB (10,007 total segments)
*   **Target dictionary:** `x12_00501.json` (~54 MB on disk)

| Scenario | Processing Time | Approx. Process Memory |
| :--- | :--- | :--- |
| **Parse (No Dictionary)** | 21 ms | 58 MB |
| **Parse (Cold Dict Load & Enrich)**| 455 ms | 179 MB |
| **Parse (Cached Dict Enrich)** | 385 ms | 179 MB |

*Note: Loading the 54 MB X12 dictionary into the internal `ConcurrentDictionary` adds ~120 MB to the process working set, allocating millions of string keys for qualifier lookups.*

## EDIFACT Baseline (30,000 Segments)
*   **Input file size:** ~549 KB (30,005 total segments)
*   **Target dictionary:** `edifact_D96A.json` (~3.9 MB on disk)

| Scenario | Processing Time | Approx. Process Memory |
| :--- | :--- | :--- |
| **Parse (No Dictionary)** | 37 ms | 69 MB |
| **Parse (Cold Dict Load & Enrich)**| 104 ms | 82 MB |
| **Parse (Cached Dict Enrich)** | 87 ms | 82 MB |

*Note: The EDIFACT D96A dictionary is substantially smaller than modern X12 dictionaries, resulting in a much lighter memory footprint (an increase of only ~13 MB) and faster enrichment times.*

## Observations
- **Core Parsing (Regex-Free) is extremely fast.** Tokenizing 10k–30k segments takes only 20–40 milliseconds, proving the regex-free structural parsing approach is effective.
- **Dictionary lookups are the primary bottleneck.** Mapping tens of thousands of segment, element, and qualifier names against the cached dictionary objects adds 300–400 ms of overhead for large payloads.
- **Memory consumption scales linearly with dictionary size.** Loading multiple massive X12 dictionaries (like `00401` and `00501`) simultaneously will cause a visible increase in memory consumption, confirming the design limitation noted in `schema-coverage-and-limitations.md`.
