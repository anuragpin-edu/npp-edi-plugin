#include "PluginInterface.h"
#include <windows.h>
#include <tchar.h>

const wchar_t NPP_PLUGIN_NAME[] = L"NppEdiLexer";
NppData nppData;
FuncItem funcItem[1];

static void aboutCommand() {
    try {
        ::MessageBoxW(nppData._nppHandle, L"NppEdiLexer provides native structural lexing for EDI X12, EDIFACT, and VDA.", L"About NppEdiLexer", MB_OK);
    } catch (...) {
        // Safe boundary
    }
}

static void commandMenuInit() {
    funcItem[0]._pFunc = aboutCommand;
    wcscpy_s(funcItem[0]._itemName, L"About NppEdiLexer");
    funcItem[0]._init2Check = false;
    funcItem[0]._pShKey = NULL;
}

extern "C" __declspec(dllexport) void setInfo(NppData notepadPlusData) {
    try {
        nppData = notepadPlusData;
        commandMenuInit();
    } catch (...) {}
}

extern "C" __declspec(dllexport) const wchar_t * getName() {
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
    (void)Message;
    (void)wParam;
    (void)lParam;
    return TRUE;
}

extern "C" __declspec(dllexport) BOOL isUnicode() {
    return TRUE;
}
