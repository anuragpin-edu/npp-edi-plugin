#include <cstdlib>
#include <cassert>
#include <string>
#include <exception>

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

class LexerEdifact : public LexerBase {
public:
    LexerEdifact() {}
    virtual ~LexerEdifact() {}

    void SCI_METHOD Lex(Sci_PositionU startPos, Sci_Position length, int /*initStyle*/, IDocument *pAccess) override {
        if (!pAccess) return;
        
        try {
            LexAccessor styler(pAccess);
            styler.StartAt(startPos);
            styler.StartSegment(startPos);

            Sci_PositionU endPos = startPos + length;
            
            // EDIFACT Defaults
            char componentSeparator = ':';
            char elementSeparator = '+';
            char decimalMark = '.';
            char releaseChar = '?';
            char segmentTerminator = '\'';
            
            // Derive separators from UNA if it exists
            if (styler.Length() >= 9) {
                if (styler.SafeGetCharAt(0) == 'U' && 
                    styler.SafeGetCharAt(1) == 'N' && 
                    styler.SafeGetCharAt(2) == 'A') {
                    componentSeparator = styler.SafeGetCharAt(3);
                    elementSeparator = styler.SafeGetCharAt(4);
                    decimalMark = styler.SafeGetCharAt(5);
                    releaseChar = styler.SafeGetCharAt(6);
                    segmentTerminator = styler.SafeGetCharAt(8);
                }
            }

            Sci_PositionU pos = startPos;
            bool inSegmentId = true;
            bool isEscaped = false;
            std::string currentSegmentId = "";

            if (startPos > 0) {
                char prev = styler.SafeGetCharAt(startPos - 1);
                // Basic heuristic for line continuation
                if (prev == segmentTerminator || prev == '\n' || prev == '\r') {
                    inSegmentId = true;
                } else {
                    inSegmentId = false;
                }
            }

            while (pos < endPos) {
                char ch = styler.SafeGetCharAt(pos);

                if (isEscaped) {
                    isEscaped = false; // consume escaped char as value
                } else if (ch == releaseChar) {
                    isEscaped = true;
                } else if (ch == '\r' || ch == '\n') {
                    styler.ColourTo(pos, EdiStyles::Default);
                    inSegmentId = true;
                    currentSegmentId = "";
                    pos++;
                    continue; // Skip styling this as segment
                } else if (ch == elementSeparator || ch == componentSeparator) {
                    styler.ColourTo(pos - 1, inSegmentId ? 
                        (isControlSegment(currentSegmentId) ? EdiStyles::Control : EdiStyles::SegmentId) 
                        : EdiStyles::Value);
                    styler.ColourTo(pos, EdiStyles::Separator);
                    inSegmentId = false;
                } else if (ch == segmentTerminator) {
                    styler.ColourTo(pos - 1, inSegmentId ? 
                        (isControlSegment(currentSegmentId) ? EdiStyles::Control : EdiStyles::SegmentId) 
                        : EdiStyles::Value);
                    styler.ColourTo(pos, EdiStyles::Separator);
                    inSegmentId = true;
                    currentSegmentId = "";
                } else {
                    if (inSegmentId) {
                        currentSegmentId += ch;
                        if (currentSegmentId.length() > 3) {
                            inSegmentId = false;
                        }
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

private:
    bool isControlSegment(const std::string& id) {
        return id == "UNA" || id == "UNB" || id == "UNH" || id == "UNT" || id == "UNZ";
    }
};

extern const LexerModule lmEdiEdifact(SCLEX_AUTOMATIC, []() -> ILexer5* { return new LexerEdifact(); }, "edi_edifact", nullptr);
