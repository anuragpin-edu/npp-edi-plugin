# Notepad++ EDI Plugin Development Handoff

Prepared for: Gemini / Google Antigravity kickoff  
Project: Free, offline Notepad++ plugin for EDI inspection  
Date: 2026-07-01  
Status: Development kickoff document

## 1. Executive Decision

Start development now, but do not let X12 or VDA block the kickoff.

Phase 1 should build the reusable Notepad++ plugin shell, parser architecture, dockable tree panel, navigation, validation pipeline, and EDIFACT-first proof of concept. X12 and VDA should run as separate research/dictionary tracks and enter implementation only after licensing and source-of-truth questions are resolved.

The practical MVP is:

- Notepad++ plugin loads successfully on current 64-bit Notepad++.
- User opens a local EDI file.
- Plugin detects EDIFACT from `UNA` / `UNB`.
- Plugin parses segments and elements.
- Dockable panel shows a collapsible segment/element tree.
- Clicking a tree node jumps to the raw segment in the editor.
- Basic validation flags malformed delimiters, missing `UNH` / `UNT`, and segment-count mismatch.
- Prettify / minify works for EDIFACT.
- All processing remains local and offline.

Hold until Phase 2/3:

- X12 dictionary coverage.
- X12 997/999 generation.
- VDA message coverage.
- Partner-specific implementation guide validation.
- Any redistribution of Bots-derived grammar data.

## 2. Why X12 and VDA Should Be Held

X12 and VDA are not hard because of parsing alone. They are hard because of dictionary provenance, version coverage, and licensing.

Bots-EDI has grammar archives for X12 and EDIFACT, including X12 004010, 005010, 006020, and EDIFACT directory archives. The Bots site states that grammars define input/output file structure and that ready grammars exist for many EDIFACT and X12 standard documents. SourceForge also lists separate X12 and EDIFACT grammar archives.

However, Bots-derived packages are GPL. A PyPI package extracted from Bots explicitly states GPLv3 licensing.

Therefore:

- Do not embed Bots grammar files in the plugin until the license strategy is chosen.
- Do not copy Bots grammar structures into plugin source as if they are neutral data.
- Use Bots as a research reference only, unless the whole plugin intentionally adopts GPL-compatible licensing.
- For Phase 1, create a small hand-authored EDIFACT fixture dictionary covering only a few test messages/segments needed to validate the UI and parser.

VDA should be even later because public, complete, free VDA grammar coverage is uncertain.

## 3. Development Strategy

Build the plugin in layers so standards can be added later without rewriting the shell.

### Layer 1: Notepad++ Host Integration
### Layer 2: Core Domain Model
### Layer 3: Standard Detection
### Layer 4: Parser Implementations
### Layer 5: Dictionary Provider

See full handoff document for detailed layer specifications.

## 4. Phase Roadmap

- **Phase 0**: Research Lockdown and Repo Setup
- **Phase 1**: EDIFACT MVP
- **Phase 2**: X12 Track (held)
- **Phase 3**: VDA Track (held)

## 5. Constraints

- No cloud services.
- No Bots grammar file embedding.
- No X12/VDA implementation in Phase 1.
- No real trading partner data.
- Parser core independent from Notepad++ UI.
- All processing local and offline.
