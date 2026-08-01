# NppEdiLexer (PoC-A)

This directory contains the native C++ Scintilla/Lexilla Lexer module for NppEdiPlugin.

## Features (PoC-A)
- Exposes standard Scintilla/Lexilla exports: `GetLexerCount`, `GetLexerName`, `CreateLexer`
- Registers `edi_x12`, `edi_edifact`, `edi_vda`
- Minimal structural styling (segment IDs, separators, values, control segments)
- Uses pinned Lexilla 5.5.1 and Scintilla 5.6.4 headers

## Build Requirements
- CMake 3.14+
- MSVC (Windows) or GCC/Clang (Linux/macOS) for testing

The Lexer is packaged with NppEdiPlugin and placed in the plugin directory. Notepad++ >= 8.4 natively loads any DLL inside the plugin folder that exports Lexilla methods.
