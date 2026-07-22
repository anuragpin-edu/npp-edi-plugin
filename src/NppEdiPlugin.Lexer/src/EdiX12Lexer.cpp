#include <cstdlib>
#include <cassert>
#include <string>
#include <exception>

#include "ILexer.h"
#include "Scintilla.h"
#include "SciLexer.h"
#include "LexAccessor.h"
#include "LexerModule.h"
#include "LexerBase.h"
#include "EdiStyles.h"

using namespace Scintilla;
using namespace Lexilla;

class LexerX12 : public LexerBase {
public:
    LexerX12() {}
    virtual ~LexerX12() {}

    void SCI_METHOD Lex(Sci_PositionU startPos, Sci_Position length, int initStyle, IDocument *pAccess) override {
        if (!pAccess) return;
        
        try {
            LexAccessor styler(pAccess);
            styler.StartAt(startPos);
            styler.StartSegment(startPos);

            Sci_PositionU endPos = startPos + length;
            
            // X12 Defaults
            char elementSeparator = '*';
            char segmentTerminator = '~';
            
            // Derive separators from ISA if it exists at the start of document
            // A complete ISA is 106 bytes long. ISA segment terminator is at position 105.
            if (styler.Length() >= 106) {
                char header[4];
                header[0] = styler.SafeGetCharAt(0);
                header[1] = styler.SafeGetCharAt(1);
                header[2] = styler.SafeGetCharAt(2);
                header[3] = '\0';
                
                if (header[0] == 'I' && header[1] == 'S' && header[2] == 'A') {
                    elementSeparator = styler.SafeGetCharAt(3);
                    segmentTerminator = styler.SafeGetCharAt(105);
                }
            }

            // Simple state machine for segment ID, value, separator
            int state = initStyle;
            if (state == SCE_EDI_DEFAULT) {
                // Determine if we are starting a segment ID
                // Just fallback to default state
            }
            
            Sci_PositionU pos = startPos;
            bool inSegmentId = true;
            std::string currentSegmentId = "";

            if (startPos > 0) {
                // Simple heuristic: if previous char was a segment terminator or newline, we start a segment
                char prev = styler.SafeGetCharAt(startPos - 1);
                if (prev == segmentTerminator || prev == '\n' || prev == '\r') {
                    inSegmentId = true;
                } else {
                    inSegmentId = false;
                }
            }

            while (pos < endPos) {
                char ch = styler.SafeGetCharAt(pos);

                if (ch == '\r' || ch == '\n') {
                    styler.ColourTo(pos, SCE_EDI_DEFAULT);
                    inSegmentId = true;
                    currentSegmentId = "";
                } else if (ch == elementSeparator) {
                    styler.ColourTo(pos - 1, inSegmentId ? 
                        (isControlSegment(currentSegmentId) ? SCE_EDI_CONTROL : SCE_EDI_SEGMENT_ID) 
                        : SCE_EDI_VALUE);
                    styler.ColourTo(pos, SCE_EDI_SEPARATOR);
                    inSegmentId = false;
                } else if (ch == segmentTerminator) {
                    styler.ColourTo(pos - 1, inSegmentId ? 
                        (isControlSegment(currentSegmentId) ? SCE_EDI_CONTROL : SCE_EDI_SEGMENT_ID) 
                        : SCE_EDI_VALUE);
                    styler.ColourTo(pos, SCE_EDI_SEPARATOR);
                    inSegmentId = true;
                    currentSegmentId = "";
                } else {
                    if (inSegmentId) {
                        currentSegmentId += ch;
                        if (currentSegmentId.length() > 3) {
                            inSegmentId = false; // Segment IDs are max 3 chars
                        }
                    }
                }
                pos++;
            }
            styler.ColourTo(endPos - 1, SCE_EDI_VALUE); // color remainder
            styler.Flush();
        } catch (...) {
            // Swallow all C++ exceptions across DLL boundary
        }
    }

    void SCI_METHOD Fold(Sci_PositionU, Sci_Position, int, IDocument*) override {}

    void * SCI_METHOD PrivateCall(int, void *) override { return nullptr; }

    int SCI_METHOD LineEndTypesSupported() override { return 0; }

    int SCI_METHOD PrimaryStyleFromStyle(int style) override { return style; }

private:
    bool isControlSegment(const std::string& id) {
        return id == "ISA" || id == "GS" || id == "ST" || id == "SE" || id == "GE" || id == "IEA";
    }
};

extern "C" LexerModule lmEdiX12(SCLEX_AUTOMATIC, []() -> ILexer5* { return new LexerX12(); }, "edi_x12", nullptr);
