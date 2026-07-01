# Phase Roadmap

NppEdiPlugin development is organized into sequential phases. Each phase has clear scope boundaries and exit criteria.

---

## Phase 0 — Project Scaffold ✅

**Goal**: Repository structure, build pipeline, core model library.

### Deliverables
- [x] Repository with README, LICENSE, DICTIONARY_PROVENANCE
- [x] `Edi.Core` project with model classes and interfaces
- [x] GitHub Actions CI (build + test)
- [x] Architecture and roadmap documentation

### Exit Criteria
- `dotnet build` succeeds for `Edi.Core`
- `dotnet test` passes (even if zero tests initially)
- All foundation documents are in place

---

## Phase 1 — EDIFACT MVP 🔨

**Goal**: A working Notepad++ plugin that can parse, display, format, and validate EDIFACT documents.

### Deliverables
- [ ] `Edi.Edifact` library — UNA-aware parser, built-in dictionary (10 segments), formatter, basic validator
- [ ] `NppEdiPlugin` — Notepad++ plugin shell with docking panel
- [ ] Tree view showing segment → element → component hierarchy
- [ ] Click-to-navigate between tree and source text (bidirectional)
- [ ] Prettify (one segment per line, indented) and Minify (compact)
- [ ] Structural validation with inline error markers
- [ ] Human-readable labels from built-in dictionary
- [ ] Unit tests with ≥80% coverage on parser and formatter
- [ ] Hand-authored sample EDI files for testing

### Exit Criteria
- Plugin loads in 64-bit Notepad++ v8.4+ without errors
- Parses ORDERS and INVOIC sample messages correctly
- Tree view renders and navigates accurately
- Prettify ↔ Minify round-trips without data loss
- Validation catches: missing UNB/UNZ, mismatched segment counts, unknown segments
- All tests pass in CI

---

## Phase 2 — X12 Support 📋

**Goal**: Add ASC X12 parsing alongside EDIFACT.

### Deliverables
- [ ] `Edi.X12` library — ISA/GS/ST parser, X12 dictionary, formatter, validator
- [ ] Auto-detection: plugin detects whether document is EDIFACT or X12
- [ ] Tree view and formatting work for X12 documents
- [ ] X12-specific validation rules
- [ ] Dictionary provenance documented for X12

### Exit Criteria
- X12 810, 850, 856 sample messages parse correctly
- Auto-detection is ≥95% accurate on test corpus
- No regressions in EDIFACT functionality
- All tests pass in CI

---

## Phase 3 — VDA & Polish 📋

**Goal**: Add VDA support and polish the user experience.

### Deliverables
- [ ] `Edi.Vda` library — VDA fixed-width parser, dictionary, formatter
- [ ] User preferences (default delimiters, theme, panel position)
- [ ] Search within parsed tree
- [ ] Export tree to JSON/XML
- [ ] Performance optimization for large files (>1 MB)
- [ ] Plugin settings dialog

### Exit Criteria
- VDA 4905, 4913 sample messages parse correctly
- Plugin handles files up to 5 MB without noticeable lag
- Settings persist across Notepad++ sessions
- All tests pass in CI

---

## Decision Log

| Date | Decision | Rationale |
|---|---|---|
| 2026-07-01 | Target .NET Standard 2.0 for core libraries | Maximum compatibility with .NET Framework 4.8 plugin and .NET 8 tests |
| 2026-07-01 | Hand-author dictionary from public specs | Avoid GPL contamination from Bots-EDI |
| 2026-07-01 | Phase 1 = EDIFACT only | Most common standard in target user base; reduces scope risk |
