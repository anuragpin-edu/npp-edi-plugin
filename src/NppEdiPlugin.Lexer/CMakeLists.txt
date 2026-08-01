cmake_minimum_required(VERSION 3.14)
project(NppEdiLexer LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

include(FetchContent)

# Pin Scintilla 5.6.4
FetchContent_Declare(
    scintilla
    URL "https://prdownloads.sourceforge.net/scintilla/scintilla564.zip?download"
    URL_HASH SHA256=3ffd69532649556978cdbe7ebe1293d9cf3259b290a7974a71e77a32bac50e12
)

# Pin Lexilla 5.5.1
FetchContent_Declare(
    lexilla
    URL "https://github.com/ScintillaOrg/lexilla/archive/refs/tags/rel-5-5-1.zip"
    URL_HASH SHA256=5c147f927927b35c55c630ef3a9affd5708400a3cad80447cc1043ab9a816f31
)

# Pin nppPluginTemplate (for Notepad++ Plugin API headers)
FetchContent_Declare(
    nppPluginTemplate
    URL "https://github.com/npp-plugins/plugintemplate/archive/987553015dc12caf2ca0e49617ba1f0e2d17faae.zip"
    URL_HASH SHA256=634173fb8e86ff8bed36eba5c540d804f6b1dd348bc89a2b819d0546f894de0a
)

FetchContent_MakeAvailable(scintilla lexilla nppPluginTemplate)

add_library(NppEdiLexer SHARED
    src/PluginExports.cpp
    src/LexerExports.cpp
    src/EdiX12Lexer.cpp
    src/EdiEdifactLexer.cpp
    src/EdiVdaLexer.cpp
)

target_include_directories(NppEdiLexer PRIVATE
    include
    src
    ${scintilla_SOURCE_DIR}/include
    ${lexilla_SOURCE_DIR}/include
    ${lexilla_SOURCE_DIR}/lexlib
    ${nppplugintemplate_SOURCE_DIR}/src
)

# Compile necessary lexlib files directly:
target_sources(NppEdiLexer PRIVATE
    ${lexilla_SOURCE_DIR}/lexlib/Accessor.cxx
    ${lexilla_SOURCE_DIR}/lexlib/CharacterCategory.cxx
    ${lexilla_SOURCE_DIR}/lexlib/CharacterSet.cxx
    ${lexilla_SOURCE_DIR}/lexlib/LexerBase.cxx
    ${lexilla_SOURCE_DIR}/lexlib/LexerModule.cxx
    ${lexilla_SOURCE_DIR}/lexlib/LexerSimple.cxx
    ${lexilla_SOURCE_DIR}/lexlib/WordList.cxx
    ${lexilla_SOURCE_DIR}/lexlib/PropSetSimple.cxx
)

if (MSVC)
    target_compile_options(NppEdiLexer PRIVATE /EHsc /sdl /guard:cf /W4)
    target_link_options(NppEdiLexer PRIVATE /GUARD:CF)
    target_compile_definitions(NppEdiLexer PRIVATE UNICODE _UNICODE)
    
    # We must export the 3 functions.
    # Alternatively we could use a .def file, but we'll use __declspec(dllexport) in LexerExports.cpp.
else()
    target_compile_options(NppEdiLexer PRIVATE -Wall -Wextra -fstack-protector-strong)
endif()

# We might also want to build the native tests
enable_testing()
add_executable(NativeLexerTests tests/NativeLexerTests.cpp)
target_include_directories(NativeLexerTests PRIVATE
    include
    src
    ${scintilla_SOURCE_DIR}/include
    ${lexilla_SOURCE_DIR}/include
    ${lexilla_SOURCE_DIR}/lexlib
)
# We can link the shared library, or recompile the lexer sources into the test
target_sources(NativeLexerTests PRIVATE
    src/LexerExports.cpp
    src/EdiX12Lexer.cpp
    src/EdiEdifactLexer.cpp
    src/EdiVdaLexer.cpp
    ${lexilla_SOURCE_DIR}/lexlib/Accessor.cxx
    ${lexilla_SOURCE_DIR}/lexlib/CharacterCategory.cxx
    ${lexilla_SOURCE_DIR}/lexlib/CharacterSet.cxx
    ${lexilla_SOURCE_DIR}/lexlib/LexerBase.cxx
    ${lexilla_SOURCE_DIR}/lexlib/LexerModule.cxx
    ${lexilla_SOURCE_DIR}/lexlib/LexerSimple.cxx
    ${lexilla_SOURCE_DIR}/lexlib/WordList.cxx
    ${lexilla_SOURCE_DIR}/lexlib/PropSetSimple.cxx
)

add_test(NAME NativeLexerTests COMMAND NativeLexerTests)

add_executable(NativePluginAbiTests tests/NativePluginAbiTests.cpp)
target_include_directories(NativePluginAbiTests PRIVATE
    include
    src
    ${nppplugintemplate_SOURCE_DIR}/src
    ${scintilla_SOURCE_DIR}/include
    ${lexilla_SOURCE_DIR}/include
    ${lexilla_SOURCE_DIR}/lexlib
)
target_sources(NativePluginAbiTests PRIVATE
    src/PluginExports.cpp
    src/LexerExports.cpp
    src/EdiX12Lexer.cpp
    src/EdiEdifactLexer.cpp
    src/EdiVdaLexer.cpp
    ${lexilla_SOURCE_DIR}/lexlib/Accessor.cxx
    ${lexilla_SOURCE_DIR}/lexlib/CharacterCategory.cxx
    ${lexilla_SOURCE_DIR}/lexlib/CharacterSet.cxx
    ${lexilla_SOURCE_DIR}/lexlib/LexerBase.cxx
    ${lexilla_SOURCE_DIR}/lexlib/LexerModule.cxx
    ${lexilla_SOURCE_DIR}/lexlib/LexerSimple.cxx
    ${lexilla_SOURCE_DIR}/lexlib/WordList.cxx
    ${lexilla_SOURCE_DIR}/lexlib/PropSetSimple.cxx
)
target_compile_definitions(NativePluginAbiTests PRIVATE UNICODE _UNICODE)

add_test(NAME NativePluginAbiTests COMMAND NativePluginAbiTests)
