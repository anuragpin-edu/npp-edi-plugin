# Schema Importer

This tool converts upstream EDI schemas from `vscode-edi-support` into the `NppEdiPlugin` internal dictionary format.

## Usage

```bash
python3 import_schemas.py --source <path-to-schemas> --output <output-dir> --standard <x12|edifact|all> [--release <optional-release>]
```

### Examples

Import all X12 schemas:
```bash
python3 import_schemas.py --source /tmp/vscode-edi-support/packages/edi-parser/src/schemas/ --output ../../src/Edi.Dictionaries/Data/ --standard x12
```

Import a specific EDIFACT release:
```bash
python3 import_schemas.py --source /tmp/vscode-edi-support/packages/edi-parser/src/schemas/ --output ../../src/Edi.Dictionaries/Data/ --standard edifact --release D96A
```

## Details
- Reads standard upstream JSON files (Segments, Qualifiers, DocumentTypes).
- Translates `QualifierRef` to inline codes on elements.
- Uses `meta.ts` files to assign friendly names to messages (e.g. 850 = Purchase Order).
- Generates fully self-contained `{standard}_{release}.json` files sorted deterministically.
- Generates `import-manifest.json` detailing the converted files and their origins.
