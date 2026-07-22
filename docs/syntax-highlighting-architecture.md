# Syntax Highlighting Architecture — NppEdiPlugin

> **Status**: Architecture planning — ready for PoC  
> **Target branch**: `feature/edi-syntax-highlighting`  
> **Upstream reference**: Deric Lee's VS Code EDI Support (MIT)  
> Repo: `https://github.com/hellooops/vscode-edi-support`  
> Pinned commit: `10d39d2495a1a8b5264bf35c311a1e07efa745e7`

---

## 1. Text-Offset Model & UTF-16 Conversion

All host-independent types operate in **UTF-16 code units** — the same coordinate space as a C# `string`. 

```csharp
public readonly struct EdiTextSpan : IEquatable<EdiTextSpan>
{
    public int Utf16Start  { get; }
    public int Utf16Length { get; }
    public int Utf16End    => Utf16Start + Utf16Length;
}
```

**Notepad++ Adapter Responsibility & Line-based Caching**:
The managed adapter bridging `IEdiSemanticService` to Scintilla converts UTF-16 positions to the active Scintilla encoding using cached line-based mappings (UTF-16 line start to Scintilla byte line start). It invalidates only affected lines after edits and does not scan the whole document prefix repeatedly. 

**Non-ASCII Testing**: 
Span test suites must include non-ASCII free-text values before and inside styled elements to ensure offsets are correct.

---

## 2. Lexer Architecture Separation

### Native Lexilla DLL (C++)
The official Notepad++ external lexer interface (`ILexer5`) is the primary architecture. The native C++ DLL exclusively owns editor-facing structural lexing and all base Scintilla style bytes. It runs synchronously on the UI thread during `Lex()`. There will be no parallel structural lexer implementations in C#.

**Native Lexer Styles (Structural & Lightweight)**:
- Envelope/control (ISA, UNB, 511, etc.)
- Segment/record ID
- Separators
- Number/date (where structurally identifiable without complex JSON dictionaries)
- Ordinary value
- VDA filler/reserved fields
- Structural invalid syntax (e.g. record length mismatches)

**Native Safety Requirements**:
The native lexer must never crash Notepad++. It must gracefully handle empty/partial documents, incomplete ISA/UNA, malformed VDA, unknown record types, invalid encodings, very long records, and unexpected positions. Fuzz-like malformed-input tests and integer-boundary validation will be added.

### Managed Plugin (C#)
The managed `Edi.Core` owns rich parsing, dictionary lookup, formatting, semantic information, and validation.

**Managed Semantic Features**:
- Hover/call tips
- Qualifier meanings
- Element and segment definitions
- Validation indicators (squiggles)
- Optional annotations (ghost text)

**Style Ownership Rules**:
The native lexer owns all style bytes. The managed plugin **must not** asynchronously overwrite native lexer styles with `SCI_STARTSTYLING` after dictionary enrichment, as native recolorization would cause conflicting style ownership. Schema-driven qualifier coloring is deferred until a safe native-readable schema/index strategy is designed.

**Duplicated Logic Prevention**:
Declarative rules (envelope tag lists, default delimiters, semantic role IDs, VDA metadata) will be documented and shared deliberately.

---

## 3. Managed Auto-Detection & UI-Thread Rules

### Auto-Detection Responsibility
Auto-detection is a managed-plugin responsibility. The native lexer merely styles the language assigned to it.
The managed plugin will conservatively assign `EDI X12`, `EDI EDIFACT`, or `EDI VDA` after a buffer is opened/activated.
- It must **not** override explicit user language selections.
- It must remember manual Plain Text selection and avoid repeated switching.
- It must use bounded-prefix detection and require strong evidence for VDA.

### Strict UI-Thread Rules
- Background workers process **immutable text snapshots only** and do not call Scintilla or Notepad++ APIs.
- Before applying a deferred result (like a call tip or validation indicator) on the UI thread, verify:
  1. Same buffer ID
  2. Same document modification version
  3. Same selected EDI language
- Discard stale results.

### Lazy Dictionary Loading by Release
Large JSON dictionaries are loaded lazily by release, outside of lexer callbacks, and cached per release. The PoC remains entirely structural.

---

## 4. Implementation Roadmap & Proof-of-Concept

### Proof-of-Concept Sequence

**PoC-A: Native Lexilla Baseline**
- Create a minimal native Lexilla DLL exporting `GetLexerCount`, `GetLexerName`, and `CreateLexer`.
- Register `EDI X12`, `EDI EDIFACT`, and `EDI VDA`.
- Manually select each under the Language menu.
- Color structural tokens (segment ID, separator, ordinary value).
- Test editing, tabs, undo, switching to Plain Text, restart, and light/dark mode.
*(Do not begin dictionaries, hover, validation, auto-detection, or annotations during PoC-A.)*

**PoC-B: Integration & Packaging**
- Prove Style Configurator behavior for the native lexer.
- Prove packaging beside the managed plugin (document exact location and file naming for Notepad++ 8.9.7 discovery).
- Prove x64 loading in a clean Notepad++ 8.9.7 environment.

**Post-PoC Decision Record**
After PoC-A and PoC-B, explicitly document:
- Exact lexer DLL installation path.
- Actual Language-menu labels.
- Style Configurator behavior.
- Session persistence and tab-switch behavior.
- How properties/styles are exposed.
- Whether XML configuration is required beside the DLL.
- Native DLL vs managed plugin loading order.

**PoC-C: Auto-Detection**
- Add conservative managed automatic detection only after manual language selection is proven reliable.

### Milestone Sequence (Post-PoC)

Do not begin L1–L7 until the real Windows PoC establishes the post-PoC decision record facts.

1. **L1 — Managed Semantic Foundation**: Semantic models, UTF-16 spans, dictionary metadata, hover contracts, validation contracts, and non-ASCII tests.
2. **L2 — X12 Structural Lexer**: Native/C++ scanning, delimiter invalidation rules. *Includes X12 native lexer automated tests.*
3. **L3 — EDIFACT Structural Lexer**: Native/C++ UNA and release character rules. *Includes EDIFACT native lexer automated tests.*
4. **L4 — VDA Structural Lexer**: Native/C++ fixed-width slicing and line invalidation. *Includes VDA native lexer automated tests.*
5. **L5 — Editor Integration**: Stabilization of the native lexer + managed plugin hybrid. *Includes integration/state tests.*
6. **L6 — Schema Enrichment**: Managed schema enrichment builds per-buffer semantic indexes for qualifier descriptions, element and segment metadata, hover/call tips, validation, and optional annotations. It **must not replace or asynchronously overwrite native lexer style bytes.** *Includes dictionary/cache tests.*
7. **L7 — Call tips (hover)**: `SCN_DWELLSTART`, qualifier code resolution. *Includes hover lookup tests.*
8. **L8 — Final Regression Coverage**: Cross-standard regression test suite spanning structural and semantic outputs.
9. **L9 — Optional annotations**: Margin/ghost text (disabled by default).
10. **L10 — Validation indicators**: Background validation mapping to squiggles.
