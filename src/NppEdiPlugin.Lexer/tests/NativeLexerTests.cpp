#include <iostream>
#include <string>
#include <cstring>
#include "ILexer.h"

#ifdef _WIN32
#define EXPORT_CC __stdcall
#else
#define EXPORT_CC
#endif

extern "C" {
    int EXPORT_CC GetLexerCount();
    void EXPORT_CC GetLexerName(unsigned int index, char *name, int buflength);
    Scintilla::ILexer5* EXPORT_CC CreateLexer(const char *name);
}

int main() {
    int count = GetLexerCount();
    if (count != 3) {
        std::cerr << "Expected 3 lexers, got " << count << "\n";
        return 1;
    }

    bool foundX12 = false;
    bool foundEdifact = false;
    bool foundVda = false;

    for (int i = 0; i < count; ++i) {
        char name[256];
        GetLexerName(i, name, sizeof(name));
        std::string sname(name);
        
        if (sname == "edi_x12") foundX12 = true;
        else if (sname == "edi_edifact") foundEdifact = true;
        else if (sname == "edi_vda") foundVda = true;
        else {
            std::cerr << "Unexpected lexer name: " << sname << "\n";
            return 1;
        }
        
        Scintilla::ILexer5* lexer = CreateLexer(name);
        if (!lexer) {
            std::cerr << "Failed to create lexer: " << sname << "\n";
            return 1;
        }
        
        // Very basic lexer validation: check it exists and can be deleted
        lexer->Release();
    }

    if (!foundX12 || !foundEdifact || !foundVda) {
        std::cerr << "Missing one or more expected lexers\n";
        return 1;
    }

    // Edge cases
    Scintilla::ILexer5* invalidLexer = CreateLexer("invalid_lexer");
    if (invalidLexer) {
        std::cerr << "Created lexer for invalid name!\n";
        return 1;
    }

    Scintilla::ILexer5* nullLexer = CreateLexer(nullptr);
    if (nullLexer) {
        std::cerr << "Created lexer for null name!\n";
        return 1;
    }

    std::cout << "NativeLexerTests passed successfully.\n";
    return 0;
}
