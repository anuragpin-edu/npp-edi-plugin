#include "PluginInterface.h"
#include <windows.h>
#include <tchar.h>

const TCHAR NPP_PLUGIN_NAME[] = TEXT("NppEdiLexer");
NppData nppData;
FuncItem funcItem[1];

static void aboutCommand() {
    try {
        ::MessageBox(nppData._nppHandle, TEXT("NppEdiLexer provides native structural lexing for EDI X12, EDIFACT, and VDA."), TEXT("About NppEdiLexer"), MB_OK);
    } catch (...) {
        // Safe boundary
    }
}

static void commandMenuInit() {
    funcItem[0]._pFunc = aboutCommand;
    lstrcpy(funcItem[0]._itemName, TEXT("About NppEdiLexer"));
    funcItem[0]._init2Check = false;
    funcItem[0]._pShKey = NULL;
}

extern "C" __declspec(dllexport) void setInfo(NppData notepadPlusData) {
    try {
        nppData = notepadPlusData;
        commandMenuInit();
    } catch (...) {}
}

extern "C" __declspec(dllexport) const TCHAR * getName() {
    return NPP_PLUGIN_NAME;
}

extern "C" __declspec(dllexport) FuncItem * getFuncsArray(int *nbF) {
    try {
        *nbF = 1;
        return funcItem;
    } catch (...) {
        *nbF = 0;
        return nullptr;
    }
}

extern "C" __declspec(dllexport) void beNotified(SCNotification *notifyCode) {
    (void)notifyCode;
}

extern "C" __declspec(dllexport) LRESULT messageProc(UINT Message, WPARAM wParam, LPARAM lParam) {
    return TRUE;
}

extern "C" __declspec(dllexport) BOOL isUnicode() {
    return TRUE;
}
