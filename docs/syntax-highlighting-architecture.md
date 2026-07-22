# Syntax Highlighting Architecture — NppEdiPlugin

> **Status**: Planning — not yet implemented  
> **Upstream reference**: Deric Lee's VS Code EDI Support (MIT)  
> Repo: https://github.com/hellooops/vscode-edi-support  
> Pinned commit: `10d39d2495a1a8b5264bf35c311a1e07efa745e7`

---

## Lessons Adapted from Deric Lee's VS Code EDI Support

### Upstream Architecture Overview

The upstream extension uses a two-layer design:

| Layer | Package | Purpose |
|---|---|---|
| Parser + Schema | `packages/edi-parser` | Pure TypeScript: parsing, schema loading, semantic enrichment, offset tracking |
| VS Code Providers | `src/providers/` | VS Code API integration: semantic tokens, hover, inlay hints, folding, document symbols |

The clean separation between the host-independent parser and the VS Code UI layer is the most directly transferable lesson. Our C# design mirrors this exactly: `Edi.Core` / `Edi.Edifact` / `Edi.X12` / `Edi.Dictionaries` form the host-independent layer, and `NppEdiPlugin` is the host-binding layer.

#### Semantic Token Classification

Upstream token types (from `semanticTokensProvider.ts`):

```typescript
const TokenTypes = {
  EdiSegmentId:                "edisupportsegmentid",
  EdiSegmentSeparator:         "edisupportseparator",
  EdiDataElementSeparator:     "edisupportseparator",
  EdiComponentElementSeparator:"edisupportseparator",
  EdiComment:                  "edisupportcomment",
  EdiValueTypeNumber:          "edisupportvaluetypenumber",
  EdiValueTypeDatetime:        "edisupportvaluetypedatetime",
  EdiValueTypeQualifier:       "edisupportvaluetypequalifier",
  EdiValueTypeOther:           "edisupportvaluetypeother",
};
```

Upstream schema-driven classification logic:

```typescript
if (element?.ediReleaseSchemaElement?.qualifierRef)    → qualifier
else if (dataType === "N" || dataType === "R")         → number
else if (dataType === "DT")                            → datetime
else                                                   → other
```

The upstream approach demonstrates the correct principle: **token role must come from schema metadata, not from value shape**. An element value `"9"` might be a numeric quantity, a message function code qualifier (EDIFACT BGM03), or an identification code qualifier depending on schema context.

#### Schema-Driven Type Styling

Upstream `ediReleaseSchemaElement` carries:

- `id` — element reference ID (e.g. `"353"` for ISA13)
- `dataType` — `"AN"`, `"DT"`, `"TM"`, `"N"`, `"R"`, `"ID"`, `"B"`
- `minLength`, `maxLength`
- `desc` — human description
- `definition` — extended definition
- `qualifierRef` — name of qualifier table if this element holds a code
- `getCodes()` — code list from schema
- `getCodeByValue(value)` — resolve a specific code to its description

The upstream code only falls back to `EdiValueTypeOther` when there is no schema metadata. When schema is available, every element value gets a typed role. We adopt the same rule.

#### Exact Positional Span Mapping

Upstream `EdiSegment` carries:

```typescript
startIndex: number   // byte offset where segment begins (absolute)
endIndex: number     // byte offset after last character before terminator
```

Upstream `EdiElement` carries:

```typescript
startIndex: number   // relative to segment start
endIndex: number     // relative to segment start
separator: string    // leading delimiter character
```

Absolute element offset = `segment.startIndex + element.startIndex`.

Our current model stores `EdiSegment.StartOffset` / `EdiSegment.EndOffset` but does not yet carry per-element or per-component absolute byte offsets. The lexer extension must add this.

#### Hover / Call-Tip Design

Upstream `HoverProviderBase.buildElementMarkdownString` constructs:
1. `**SEGID**NN (Element)` — with `Id`, `Type`, `Min/Max` badges
2. Element description + full segment rendered as code block
3. If qualifier: current value decoded + list of all available codes
4. Schema reference URL link

Upstream `HoverProviderBase.buildSegmentMarkdownString` constructs:
1. `**SEGID** (Segment)`
2. Segment description + purpose + code block

Our Notepad++ equivalent uses `SCN_DWELLSTART` + `SCI_CALLTIPSHOW` with plain-text formatting (no Markdown; no browser links).

Proposed call-tip for element `BEG02 = SA`:

```
BEG02 — Purchase Order Type Code
Value : SA
Meaning: Stand-alone Order
Type  : ID    Min/Max: 2/2
```

Proposed call-tip for segment `BEG`:

```
BEG — Beginning Segment for Purchase Order
850 Purchase Order
```

Proposed call-tip for VDA field at offset in record `711`:

```
711 Record — DeliverySchedule
Field 5 — Supplier Article Number
Offset: 35–54  Length: 20
Type: AN
```

#### Optional In-Editor Semantic Hints (Future)

Upstream inlay hints (`InlayHintsEdiProvider`) inject:
- **Segment name hints**: ghost text placed immediately after segment ID
- **Qualifier code hints**: ghost text placed after a qualifier element value

Notepad++ equivalents:

| Mechanism | Suitability |
|---|---|
| `SCI_ANNOTATIONSETTEXT` | Segment names — below the line |
| Margin text (`SCI_MARGINSETTEXT`) | Segment names — left margin |
| Call tips (on dwell) | Qualifier values — primary hover surface |
| Indicators + status bar | Validation errors |

All optional hints are **disabled by default in Phase 1**.

#### Language Detection

Upstream `package.json` first-line patterns:

```json
"x12":     "firstLine": "^ISA"
"edifact": "firstLine": "^(UNA:|UNB\\+|UNH\\+)"
"vda":     "firstLine": "^(711|511)"
```

Our conservative detection also validates VDA record length consistency. Matching only a 3-digit prefix is insufficient because X12 segment names and EDIFACT values can start with digits.

#### VDA Fixed-Width Treatment

VDA uses fixed-width records with no delimiter characters. Upstream `VdaParser` slices elements by schema-defined `[startIndex, endIndex]` offsets within each record. Record-type mismatches in length emit an `InvalidSyntax` span for the entire line.

#### Features Intentionally Not Ported

| Upstream Feature | Reason |
|---|---|
| VS Code `SemanticTokensBuilder` / `SemanticTokensLegend` | VS Code API; Notepad++ uses Scintilla style indices |
| VS Code `MarkdownString` hover | No Markdown renderer; plain-text call tips used |
| VS Code inlay hints API | No direct equivalent; approximated with annotations later |
| VS Code `FoldingRange` provider | Future work; Scintilla has `SCI_SETFOLDLEVEL` |
| VS Code `DocumentSymbol` provider | Future navigation; no outline panel in Notepad++ |
| VS Code `CompletionItem` provider | Out of scope Phase 1 |
| VS Code `CodeAction` / `CodeLens` providers | Out of scope |
| VS Code `DocumentFormatting` provider | Already have prettify/minify |
| Tree view (`treeEdiProvider.ts`) | Disabled during lexer phase |
| Schema viewer URL links | No browser integration |
| Extension marketplace assets | VS Code-specific |

---

## Semantic Token Roles

```csharp
public enum EdiSemanticRole : int
{
    Default          = 0,   // Unclassified / fallback
    EnvelopeControl  = 1,   // ISA/GS/ST/SE/GE/IEA, UNA/UNB/UNH/UNT/UNZ, VDA 511/519/711/719
    SegmentId        = 2,   // All non-envelope segment identifiers
    Separator        = 3,   // Element, component, and segment delimiters
    QualifierOrCode  = 4,   // Elements with qualifierRef in schema
    Number           = 5,   // Schema data type N or R
    DateTime         = 6,   // Schema data type DT or TM
    TextValue        = 7,   // AN / free text / untyped
    Identifier       = 8,   // Control numbers, version/release refs, ISA sender/receiver
    Comment          = 9,   // // comment lines (X12 extension)
    InvalidSyntax    = 10,  // Record length mismatch, unrecognised segment, delimiter in data
    FixedWidthFiller = 11,  // VDA padding/filler regions
    ReservedField    = 12,  // VDA reserved positions
}
```

### Classification Rules by Standard

| Token Kind | X12 | EDIFACT | VDA |
|---|---|---|---|
| Segment/record identifier | SegmentId | SegmentId | SegmentId |
| ISA/GS/ST/SE/GE/IEA | EnvelopeControl | — | — |
| UNA/UNB/UNH/UNT/UNZ | — | EnvelopeControl | — |
| 511/519/711/719 | — | — | EnvelopeControl |
| Element / component / segment separators | Separator | Separator | — |
| `qualifierRef` elements | QualifierOrCode | QualifierOrCode | QualifierOrCode |
| dataType N or R | Number | Number | Number |
| dataType DT or TM | DateTime | DateTime | DateTime |
| ISA control numbers, GS version | Identifier | — | — |
| UNB sender/receiver, UNH reference | — | Identifier | — |
| Sender, receiver, doc IDs | — | — | Identifier |
| Filler / padding regions | — | — | FixedWidthFiller |
| Reserved positions | — | — | ReservedField |
| Free text / AN without schema | TextValue | TextValue | TextValue |
| Comment lines | Comment | Comment | — |
| Mismatched length / bad syntax | InvalidSyntax | InvalidSyntax | InvalidSyntax |

---

## IEdiSemanticService — Host-Independent API

Lives in `Edi.Core.Semantics`. No dependency on Scintilla, WinForms, or any UI framework.

```csharp
namespace Edi.Core.Semantics
{
    public interface IEdiSemanticService
    {
        EdiStandard DetectStandard(string text);
        string? DetectVersionOrRelease(string text);

        /// <summary>Returns semantic token spans for the whole document or a changed region.</summary>
        IReadOnlyList<EdiSemanticSpan> GetSemanticSpans(string text, TextRange? changedRange = null);

        /// <summary>Returns full semantic info at an absolute offset (for hover/call-tip).</summary>
        EdiSemanticInfo? GetSemanticInfoAtOffset(string text, int offset);

        IReadOnlyList<EdiValidationIssue> GetValidationIssues(string text, TextRange? changedRange = null);
        EdiDocumentStructure GetDocumentStructure(string text);
    }
}
```

### EdiSemanticSpan

```csharp
public sealed class EdiSemanticSpan
{
    public int Start            { get; }  // Absolute document byte offset
    public int Length           { get; }  // Character count
    public EdiSemanticRole Role { get; }
    public EdiStandard Standard { get; }
    public string? SegmentId    { get; }  // "BEG", "BGM", "511"
    public int ElementIndex     { get; }  // 1-based; 0 = segment ID itself
    public int ComponentIndex   { get; }  // 1-based; 0 = not a component
}
```

### EdiSemanticInfo (for hover / call-tip)

```csharp
public sealed class EdiSemanticInfo
{
    public EdiStandard Standard      { get; }
    public string? Release           { get; }  // "D96A", "00501"
    public string? SegmentId         { get; }
    public string? SegmentLabel      { get; }
    public string? SegmentPurpose    { get; }
    public int ElementIndex          { get; }  // 0 = hovering segment ID
    public int ComponentIndex        { get; }
    public string? ElementId         { get; }  // "353", "C002"
    public string? ElementLabel      { get; }
    public string? ElementDef        { get; }
    public string? DataType          { get; }  // "ID", "AN", "N", "DT"
    public int? MinLength            { get; }
    public int? MaxLength            { get; }
    public string? RawValue          { get; }
    public string? QualifierDesc     { get; }  // Resolved code description
    public IReadOnlyList<EdiCodeValue>? AvailableCodes { get; }
    public int SpanStart             { get; }
    public int SpanLength            { get; }
}
```

---

## Project Structure

New project: `src/Edi.Lexer/` — `.NET Standard 2.0`, no UI dependencies.

```
Edi.Lexer/
  Edi.Lexer.csproj
  EdiSemanticRole.cs
  EdiSemanticSpan.cs
  EdiSemanticInfo.cs
  IEdiSemanticService.cs
  EdiSemanticService.cs        ← default implementation
  X12/
    X12LexerPass.cs            ← produces spans from parsed X12 document
    X12SemanticInfoResolver.cs ← resolves hover info for X12
  Edifact/
    EdifactLexerPass.cs
    EdifactSemanticInfoResolver.cs
  Vda/
    VdaLexerPass.cs
    VdaSemanticInfoResolver.cs
```

---

## Notepad++ Integration

### Scintilla Lexer Registration

Phase 1 uses `SCLEX_CONTAINER`: the plugin receives `SCN_STYLENEEDED` and applies style indices via `SCI_STARTSTYLING` / `SCI_SETSTYLING`. Style indices map directly to `(int)EdiSemanticRole`.

### Default Style Palette (Dark Mode)

| Role | Style Index | Foreground | Notes |
|---|---|---|---|
| Default | 0 | `#C8C8C8` | |
| EnvelopeControl | 1 | `#569CD6` bold | ISA/UNB/511 headers |
| SegmentId | 2 | `#4EC9B0` bold | BEG, BGM, DTM |
| Separator | 3 | `#555555` | `*`, `+`, `~`, `'` |
| QualifierOrCode | 4 | `#CE9178` | SA, ZZ, 137 |
| Number | 5 | `#B5CEA8` | Prices, quantities |
| DateTime | 6 | `#9CDCFE` | 20260721 |
| TextValue | 7 | `#D4D4D4` | Free-form text |
| Identifier | 8 | `#DCDCAA` | Control numbers |
| Comment | 9 | `#6A9955` italic | // comments |
| InvalidSyntax | 10 | `#F44747` bg `#3C1313` | Errors |
| FixedWidthFiller | 11 | `#3C3C3C` | VDA padding |
| ReservedField | 12 | `#444444` | VDA reserved |

All styles are overridable via Notepad++ Style Configurator.

### Call-Tip Integration

```csharp
// In OnNotification handler (Main.cs)
case NppMsg.SCN_DWELLSTART:
    var info = _semanticService.GetSemanticInfoAtOffset(GetEditorText(), notification.Position);
    if (info != null)
        scintilla.CallTipShow(notification.Position, FormatCallTip(info));
    break;
case NppMsg.SCN_DWELLEND:
    scintilla.CallTipCancel();
    break;
```

Call-tip width limit: 80 characters per line. Code lists truncated to 10 entries.

---

## Testing Plan

### Span Tests (`Edi.Lexer.Tests/SpanTests.cs`)

Per-span assertions:

```csharp
Assert.Equal(expectedStart,        span.Start);
Assert.Equal(expectedLength,       span.Length);
Assert.Equal(expectedRole,         span.Role);
Assert.Equal(expectedSegmentId,    span.SegmentId);
Assert.Equal(expectedElementIndex, span.ElementIndex);
```

Required coverage:

| Case | Tests |
|---|---|
| X12 standard separators | Separator role for `*` and `~` |
| X12 custom ISA separators | Custom delimiter detection |
| X12 HIPAA release fallback | EnvelopeControl + dictionary fallback |
| X12 BEG with qualifiers | QualifierOrCode for BEG01, BEG02 |
| X12 numeric element (price/qty) | Number role |
| X12 date element | DateTime role |
| EDIFACT with UNA | 5 UNA chars → Separator |
| EDIFACT default delimiters | No UNA present |
| EDIFACT escaped separator `?+` | Escape char handling |
| EDIFACT BGM function code | QualifierOrCode |
| EDIFACT DTM date value | DateTime |
| VDA 511/02 record | Fixed-width field slices |
| VDA short record | InvalidSyntax for whole record |
| VDA long record | InvalidSyntax for whole record |
| Incomplete document | Partial segments handled gracefully |
| Unknown segment | TextValue for all elements |
| Unsupported release | Structural parse only, TextValue |
| Incremental changed range | Only affected spans returned |

### Semantic Info Tests (`Edi.Lexer.Tests/SemanticInfoTests.cs`)

| Offset | Expected |
|---|---|
| `SA` in `BEG*00*SA*` | QualifierDesc = "Stand-alone Order" |
| `9` in `BGM+220+ref+9'` | QualifierDesc = "Original" |
| `BEG` segment ID | SegmentLabel populated, ElementIndex = 0 |
| `511` record ID | Record description, version |
| VDA field offset | Field name, type, start/end positions |

---

## Provenance and Licensing

### Files Studied

| File | How Used |
|---|---|
| `src/providers/semanticTokensProvider.ts` | Adapted: token category system, schema-driven type dispatch |
| `src/providers/hoverProviderBase.ts` | Adapted: element/segment hover content structure → `FormatCallTip` |
| `src/providers/hoverX12Provider.ts` | Design reference only |
| `src/providers/hoverEdifactProvider.ts` | Design reference only |
| `src/providers/hoverVdaProvider.ts` | Design reference only |
| `src/providers/inlayHintsEdiProvider.ts` | Adapted concept: optional segment-name and qualifier-code hints |
| `src/providers/foldingRangeEdiProvider.ts` | Design reference for future folding |
| `packages/edi-parser/src/parser/entities.ts` | Studied: offset model, schema linking, EdiDocument hierarchy |
| `packages/edi-parser/src/parser/ediParserBase.ts` | Studied: segment scanner, schema resolution |
| `packages/edi-parser/src/interfaces/configurations.ts` | Studied: custom schema extension model |
| `packages/edi-parser/test/x12-parser.test.ts` | Test design inspiration |
| `packages/edi-parser/test/vda-parser.test.ts` | Test design inspiration |
| `package.json` | Studied: language IDs, `firstLine` detection, semantic token scope mappings |

### What Was Not Copied

- No TypeScript or JavaScript code is included in the C# project.
- No schema JSON files from the upstream repo (we use our own schemas from the Phase 3 importer).
- No VS Code-specific API calls, extension manifests, or webpack configs.
- No screenshots, icons, or branding assets.

### Attribution for THIRD_PARTY_NOTICES.md

```
VS Code EDI Support
Source : https://github.com/hellooops/vscode-edi-support
Commit : 10d39d2495a1a8b5264bf35c311a1e07efa745e7
License: MIT
Author : Deric Lee

The semantic token classification strategy, schema-driven element type dispatch
(qualifierRef / dataType → semantic role), exact element offset span model, and
hover content structure were studied and reimplemented in C# as design inspiration.
No TypeScript source code was copied into this project.
```

---

## Phase Milestone Summary

| Milestone | Deliverable |
|---|---|
| L1 — Lexer foundation | `Edi.Lexer` project, `IEdiSemanticService`, span/info models, element-offset scanner |
| L2 — X12 spans | `X12LexerPass` producing correct spans for separators, envelope, typed elements |
| L3 — EDIFACT spans | `EdifactLexerPass` including UNA, component separator, escape handling |
| L4 — VDA spans | `VdaLexerPass` with fixed-width slicing, filler, InvalidSyntax for bad lengths |
| L5 — Scintilla integration | `SCN_STYLENEEDED` handler, style registration, default dark-mode palette |
| L6 — Call tips | `SCN_DWELLSTART` → `FormatCallTip` → `SCI_CALLTIPSHOW`; qualifier code lists |
| L7 — Tests | `Edi.Lexer.Tests` covering all span and semantic-info-at-offset cases |
| L8 — Optional hints | Annotations for segment names and qualifier codes (disabled by default) |

The tree inspector panel is **disabled** during Milestones L1–L7.
