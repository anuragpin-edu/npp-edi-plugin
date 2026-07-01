# Project Rules

This project is a free, offline Notepad++ plugin for EDI inspection.

Phase 1 scope is EDIFACT MVP only.

## Do
- Keep parser core independent from Notepad++ UI.
- Preserve source offsets for navigation.
- Write unit tests for parser and formatter behavior.
- Use hand-authored sample EDI files only.
- Document dictionary provenance.
- Use web research for SDK, licensing, and dictionary source questions.
- Target 64-bit Unicode Notepad++.
- Use C# with .NET Framework or .NET (as determined by SDK research).
- Keep all EDI processing local and offline.

## Do Not
- Add cloud services.
- Upload EDI files anywhere.
- Embed Bots grammar files unless a licensing decision explicitly allows it.
- Implement X12 or VDA in Phase 1.
- Include real trading partner data.
- Make destructive filesystem changes.
- Rewrite unrelated project files.
- Bind parser logic directly to WinForms controls.
