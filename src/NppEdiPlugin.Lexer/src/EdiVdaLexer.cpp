#include <cstdlib>
#include <cassert>
#include <string>
#include <exception>
#include <cctype>

#include "ILexer.h"
#include "Scintilla.h"
#include "SciLexer.h"
#include "LexAccessor.h"
#include "Accessor.h"
#include "PropSetSimple.h"
#include "WordList.h"
#include "LexerModule.h"
#include "LexerBase.h"
#include "EdiStyles.h"

using namespace Scintilla;
using namespace Lexilla;

class LexerVda : public LexerBase {
public:
    LexerVda() {}
    virtual ~LexerVda() {}

    void SCI_METHOD Lex(Sci_PositionU startPos, Sci_Position length, int initStyle, IDocument *pAccess) override {
        if (!pAccess) return;
        
        try {
            LexAccessor styler(pAccess);
            styler.StartAt(startPos);
            styler.StartSegment(startPos);

            Sci_PositionU endPos = startPos + length;
            Sci_PositionU pos = startPos;
            
            bool inRecordId = false;
            int recordIdCharCount = 0;

            if (startPos == 0) {
                inRecordId = true;
            } else {
                char prev = styler.SafeGetCharAt(startPos - 1);
                if (prev == '\n' || prev == '\r') {
                    inRecordId = true;
                }
            }

            while (pos < endPos) {
                char ch = styler.SafeGetCharAt(pos);

                if (ch == '\r' || ch == '\n') {
                    styler.ColourTo(pos, EdiStyles::Default);
                    inRecordId = true;
                    recordIdCharCount = 0;
                } else {
                    if (inRecordId) {
                        if (std::isdigit(static_cast<unsigned char>(ch))) {
                            recordIdCharCount++;
                            if (recordIdCharCount == 3) {
                                styler.ColourTo(pos, EdiStyles::SegmentId);
                                inRecordId = false;
                            }
                        } else {
                            // If the first characters of the line are not digits, it's malformed or not an ID
                            styler.ColourTo(pos, EdiStyles::Value);
                            inRecordId = false;
                        }
                    } else {
                        // Regular values (fixed width), styled as VALUE until the end of line
                    }
                }
                pos++;
            }
            styler.ColourTo(endPos - 1, EdiStyles::Value);
            styler.Flush();
        } catch (...) {}
    }

    void SCI_METHOD Fold(Sci_PositionU, Sci_Position, int, IDocument*) override {}

    void * SCI_METHOD PrivateCall(int, void *) override { return nullptr; }

    int SCI_METHOD LineEndTypesSupported() override { return 0; }

    int SCI_METHOD PrimaryStyleFromStyle(int style) override { return style; }
};

extern const LexerModule lmEdiVda(SCLEX_AUTOMATIC, []() -> ILexer5* { return new LexerVda(); }, "edi_vda", nullptr);
