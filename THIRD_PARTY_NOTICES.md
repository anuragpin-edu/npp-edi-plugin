# Third-Party Notices

This project uses schema definitions derived from the following sources.

## vscode-edi-support (edi-parser)

Copyright (c) 2017 Deric Lee

Repository: https://github.com/hellooops/vscode-edi-support
License: MIT
Commit: 10d39d2495a1a8b5264bf35c311a1e07efa745e7

**Schema data**: The X12 and EDIFACT schema JSON files in `src/Edi.Dictionaries/Data/`
were imported from the `packages/edi-parser/src/schemas/` directory of this repository
using the `tools/SchemaImporter/import_schemas.py` script. See `docs/SCHEMA_PROVENANCE.md`.

**Architecture design**: The semantic token classification strategy (schema-driven type
dispatch via `qualifierRef` and `dataType`), the exact element-offset span model, and
the hover/call-tip content structure described in `docs/syntax-highlighting-architecture.md`
were studied and reimplemented in C# as design inspiration. No TypeScript or JavaScript
source code from this repository was copied into the C# project. The VS Code API surface
(`SemanticTokensBuilder`, `HoverProvider`, `InlayHintsProvider`, etc.) was not ported.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

---

NppEdiPlugin is an independent open-source project and is not affiliated
with or endorsed by ASC X12, UNECE, VDA, OpenText, Liaison Technologies,
or the maintainers of vscode-edi-support.
