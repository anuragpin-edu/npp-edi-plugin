#include <cstddef>
#include "ILexer.h"
#include "LexerModule.h"

using namespace Lexilla;

// External declarations of LexerModules
extern const LexerModule lmEdiX12;
extern const LexerModule lmEdiEdifact;
extern const LexerModule lmEdiVda;

static const LexerModule* lexerModules[] = {
    &lmEdiX12,
    &lmEdiEdifact,
    &lmEdiVda
};

#ifdef _WIN32
#define EXPORT_CC __stdcall
#define EXPORT_DEF __declspec(dllexport)
#else
#define EXPORT_CC
#define EXPORT_DEF __attribute__((visibility("default")))
#endif

extern "C" {

    EXPORT_DEF int EXPORT_CC GetLexerCount() {
        return sizeof(lexerModules) / sizeof(lexerModules[0]);
    }

    EXPORT_DEF void EXPORT_CC GetLexerName(unsigned int index, char *name, int buflength) {
        if (name && buflength > 0) {
            *name = '\0';
            if (index < static_cast<unsigned int>(GetLexerCount())) {
                const char *lexName = lexerModules[index]->languageName;
                if (lexName) {
                    size_t len = 0;
                    while (lexName[len] && len < (size_t)buflength - 1) {
                        name[len] = lexName[len];
                        len++;
                    }
                    name[len] = '\0';
                }
            }
        }
    }

    EXPORT_DEF Scintilla::ILexer5* EXPORT_CC CreateLexer(const char *name) {
        if (!name) return nullptr;
        
        for (unsigned int i = 0; i < static_cast<unsigned int>(GetLexerCount()); ++i) {
            const char* lexName = lexerModules[i]->languageName;
            if (lexName) {
                const char* a = name;
                const char* b = lexName;
                while (*a && *a == *b) { ++a; ++b; }
                if (*a == '\0' && *b == '\0') {
                    return static_cast<Scintilla::ILexer5*>(lexerModules[i]->Create());
                }
            }
        }
        return nullptr;
    }

}
