# NppEdiPlugin — EDI Inspector for Notepad++

A free, offline Notepad++ plugin for inspecting **EDIFACT** documents. Provides a tree view, segment/element validation, prettify/minify, and human-readable labels — all without sending data off your machine.

> **Phase 1 — EDIFACT MVP** is currently **In Development**.

---

## Features (Phase 1 — EDIFACT)

| Feature | Status |
|---|---|
| EDIFACT segment parsing (UNA-aware) | 🔨 In Progress |
| Tree-view panel with segment/element drill-down | 🔨 In Progress |
| Prettify / Minify formatting | 🔨 In Progress |
| Segment & element labels from built-in dictionary | 🔨 In Progress |
| Basic structural validation | 🔨 In Progress |
| Click-to-navigate (tree ↔ source) | 🔨 In Progress |

## Planned

| Standard | Target Phase |
|---|---|
| X12 | Phase 2 |
| VDA | Phase 3 |

---

## Requirements

- **Notepad++** v8.4+ (64-bit, Unicode)
- **Windows** 10 / 11 or Windows Server 2016+

## Build

### Core libraries (cross-platform)

```bash
dotnet build src/Edi.Core/Edi.Core.csproj
```

### Plugin DLL (.NET Framework 4.8)

Open `NppEdiPlugin.sln` in **Visual Studio 2022** and build the plugin project targeting .NET Framework 4.8.

### Run tests

```bash
dotnet test
```

## Installation

1. Build or download the plugin DLL.
2. Copy `NppEdiPlugin.dll` into `%ProgramFiles%\Notepad++\plugins\NppEdiPlugin\`.
3. Restart Notepad++.

## Project Structure

```
src/
  Edi.Core/          Core models, parsing interfaces, dictionary, formatting (.NET Standard 2.0)
  Edi.Edifact/       EDIFACT parser & dictionary (Phase 1)
  NppEdiPlugin/      Notepad++ plugin shell (.NET Framework 4.8)
tests/
  Edi.Core.Tests/    Unit tests for core library
  Edi.Edifact.Tests/ Unit tests for EDIFACT parser
docs/
  architecture.md    Architecture overview
  phase-roadmap.md   Phase roadmap & exit criteria
```

## Privacy

All EDI processing is performed **locally and offline**. No data is uploaded, transmitted, or shared with any external service.

## License

[MIT](LICENSE) © 2026 NppEdiPlugin Contributors

## Contributing

Contributions are welcome! Please open an issue to discuss proposed changes before submitting a pull request.
