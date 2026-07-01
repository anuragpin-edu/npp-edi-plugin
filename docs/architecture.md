# Architecture

NppEdiPlugin uses a layered architecture that keeps EDI processing logic completely independent from the Notepad++ UI. This separation enables unit testing, future UI targets, and clean extension points.

---

## Layer Diagram

```
┌─────────────────────────────────────────────────┐
│  Layer 5: Notepad++ Plugin Shell                │
│  (NppEdiPlugin — .NET Framework 4.8)            │
│  Menu commands, docking panel, scintilla bridge  │
├─────────────────────────────────────────────────┤
│  Layer 4: UI Presentation                       │
│  (WinForms controls — tree view, toolbar)        │
│  No EDI logic — binds to Layer 3 view-models     │
├─────────────────────────────────────────────────┤
│  Layer 3: Application Services                  │
│  (Edi.Plugin.Core — orchestration, commands)     │
│  Coordinates parsing, formatting, validation     │
├─────────────────────────────────────────────────┤
│  Layer 2: Standard-Specific Implementations     │
│  (Edi.Edifact, future Edi.X12, Edi.Vda)         │
│  Parsers, dictionaries, validators per standard  │
├─────────────────────────────────────────────────┤
│  Layer 1: Core Models & Interfaces              │
│  (Edi.Core — .NET Standard 2.0)                 │
│  EdiDocument, EdiSegment, IEdiParser, etc.       │
└─────────────────────────────────────────────────┘
```

---

## Projects & Dependencies

| Project | Target | Depends On | Purpose |
|---|---|---|---|
| `Edi.Core` | .NET Standard 2.0 | — | Shared models, interfaces, null implementations |
| `Edi.Edifact` | .NET Standard 2.0 | Edi.Core | EDIFACT parser, dictionary, formatter, validator |
| `Edi.Plugin.Core` | .NET Standard 2.0 | Edi.Core | Application-level orchestration (parser dispatch, commands) |
| `NppEdiPlugin` | .NET Framework 4.8 | Edi.Core, Edi.Edifact, Edi.Plugin.Core | Notepad++ plugin DLL, WinForms UI |
| `Edi.Core.Tests` | .NET 8.0 | Edi.Core | Unit tests for core library |
| `Edi.Edifact.Tests` | .NET 8.0 | Edi.Edifact, Edi.Core | Unit tests for EDIFACT implementation |

### Dependency Rules

1. **Edi.Core** has **zero** external dependencies.
2. Standard-specific projects (Edi.Edifact) depend only on Edi.Core.
3. The plugin shell depends on everything but nothing depends on it.
4. **Parser logic must never reference WinForms or Notepad++ types.**
5. UI controls bind to data, never to parser internals.

---

## Data Flow

```
┌──────────────┐     raw text      ┌───────────────┐
│  Notepad++   │ ─────────────────▶│  Plugin Shell  │
│  (Scintilla) │                   │  (Layer 5)     │
└──────────────┘                   └───────┬────────┘
                                           │
                                           ▼
                                   ┌───────────────┐
                                   │  App Service   │
                                   │  (Layer 3)     │
                                   │                │
                                   │  Dispatches to │
                                   │  IEdiParser    │
                                   └───────┬────────┘
                                           │
                          ┌────────────────┼────────────────┐
                          ▼                ▼                ▼
                   ┌────────────┐  ┌────────────┐  ┌────────────┐
                   │  EDIFACT   │  │  X12       │  │  VDA       │
                   │  Parser    │  │  (Phase 2) │  │  (Phase 3) │
                   └─────┬──────┘  └────────────┘  └────────────┘
                         │
                         ▼
                   ┌────────────────┐
                   │  EdiDocument   │  (Layer 1 model)
                   │  ├─ Segments[] │
                   │  ├─ Issues[]   │
                   │  └─ Delimiters │
                   └───────┬────────┘
                           │
              ┌────────────┼────────────┐
              ▼            ▼            ▼
        ┌──────────┐ ┌──────────┐ ┌──────────┐
        │ Tree View│ │ Validate │ │ Prettify │
        │ (UI)     │ │          │ │ / Minify │
        └──────────┘ └──────────┘ └──────────┘
```

---

## Extension Points

| Extension Point | Interface | Purpose |
|---|---|---|
| New EDI standard | `IEdiParser` | Register a new parser (e.g., X12) via the dispatcher |
| Dictionary lookup | `IEdiDictionary` | Provide segment/element labels for any standard |
| Formatting | `IEdiFormatter` | Custom prettify/minify strategies |
| Validation rules | `IEdiValidator` | Add or replace validation logic |
| UI panels | Plugin shell | Add new docking panels or menu commands |

### Adding a New Standard

1. Create a new project (e.g., `Edi.X12`) referencing `Edi.Core`.
2. Implement `IEdiParser`, `IEdiDictionary`, `IEdiFormatter`, `IEdiValidator`.
3. Register the parser with `EdiParserDispatcher`.
4. No changes needed in the core or UI layers.

---

## Key Design Decisions

- **Source offsets are preserved** on every `EdiSegment` (`StartOffset`, `EndOffset`) to support click-to-navigate between tree view and source text.
- **Immutable models** — all model classes are `sealed` with get-only properties, making them safe to share across threads.
- **Null dictionary pattern** — `NullEdiDictionary` ensures parsing works without labels; dictionaries are always optional.
- **All processing is local and offline** — no network calls, no telemetry, no cloud services.
