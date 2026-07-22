#pragma once

// Define structural styles for EDI documents
enum EdiStyles {
    SCE_EDI_DEFAULT = 0,
    SCE_EDI_SEGMENT_ID = 1,
    SCE_EDI_SEPARATOR = 2,
    SCE_EDI_VALUE = 3,
    SCE_EDI_CONTROL = 4,   // Envelope/Control segments
    SCE_EDI_ERROR = 5      // Malformed syntax
};
