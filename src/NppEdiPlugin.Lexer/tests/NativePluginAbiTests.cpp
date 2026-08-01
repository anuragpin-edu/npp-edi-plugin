#include <iostream>
#include <string>
#include <windows.h>
#include <tchar.h>
#include "PluginInterface.h"

// Forward declare the exported functions we want to test
extern "C" {
    BOOL isUnicode();
    const TCHAR* getName();
    FuncItem* getFuncsArray(int* nbF);
    void setInfo(NppData notepadPlusData);
    void beNotified(SCNotification* notifyCode);
    LRESULT messageProc(UINT Message, WPARAM wParam, LPARAM lParam);
    
    // Lexer exports
    int GetLexerCount();
    void GetLexerName(unsigned int index, char* name, int buflength);
}

void assert_test(bool condition, const std::string& message) {
    if (!condition) {
        std::cerr << "FAIL: " << message << std::endl;
        exit(1);
    }
    std::cout << "PASS: " << message << std::endl;
}

int main() {
    std::cout << "Running NativePluginAbiTests..." << std::endl;

    // 1. isUnicode
    assert_test(isUnicode() == TRUE, "isUnicode() should return TRUE");

    // 2. getName
    const TCHAR* name = getName();
    assert_test(name != nullptr, "getName() should not return null");
#ifdef _UNICODE
    assert_test(wcscmp(name, L"NppEdiLexer") == 0, "getName() should return NppEdiLexer");
#else
    assert_test(strcmp(name, "NppEdiLexer") == 0, "getName() should return NppEdiLexer");
#endif

    // 3. getFuncsArray
    int nbFuncs = 0;
    FuncItem* funcs = getFuncsArray(&nbFuncs);
    assert_test(nbFuncs == 1, "getFuncsArray() should return exactly 1 command");
    assert_test(funcs != nullptr, "getFuncsArray() should not return null");
    assert_test(funcs[0]._pFunc != nullptr, "FuncItem should have a valid function pointer");

    // 4. setInfo (should not crash)
    NppData data = {0};
    setInfo(data);
    assert_test(true, "setInfo() handled safely");

    // 5. beNotified (should not crash)
    SCNotification notify = {0};
    beNotified(&notify);
    assert_test(true, "beNotified() handled safely");

    // 6. messageProc (should not crash and return TRUE)
    LRESULT res = messageProc(0, 0, 0);
    assert_test(res == TRUE, "messageProc() should return TRUE");

    // 7. GetLexerCount
    assert_test(GetLexerCount() == 3, "GetLexerCount() should return 3");

    std::cout << "All NativePluginAbiTests PASSED." << std::endl;
    return 0;
}
