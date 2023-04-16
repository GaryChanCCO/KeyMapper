// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <iostream>
#include <Windows.h>
#include <map>

using namespace std;

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

#define DLLEXPORT __declspec(dllexport)

HWND WindowHWND;
EXTERN_C DLLEXPORT void Set_WindowHWND(HWND hWnd) {
	WindowHWND = hWnd;
}

map<INT16, INT16> KeyMapDict;
EXTERN_C DLLEXPORT void Set_KeyMapDict(INT16 originalKeys[], INT16 targetKeys[], UINT32 length) {
	for (UINT32 i = 0; i < length; i++) {
		KeyMapDict[originalKeys[i]] = targetKeys[i];
	}
}


EXTERN_C DLLEXPORT void WriteGloBalVariables() {
	cout << "WindowHWND" << ": " << WindowHWND << endl;
	cout << "KeyMapDict: " << endl;
	for (auto const kvp : KeyMapDict)
	{
		cout << "    " << "0x" << hex << kvp.first << " = " << "0x" << hex << kvp.second << endl;
	}
}

EXTERN_C DLLEXPORT LRESULT KeyboardProc(_In_ int code, _In_ WPARAM wParam, _In_ LPARAM lParam) {
	//Do nothing if numlock is off
	if (!(GetKeyState(VK_NUMLOCK) & 1)) {
		return CallNextHookEx(NULL, code, wParam, lParam);
	}

	//Not mapped Keys
	if (KeyMapDict.find((INT16)wParam) == KeyMapDict.end()) {
		return CallNextHookEx(NULL, code, wParam, lParam);
	}

	bool isUp = (lParam & (1UL << 31UL)) >> 31;
	auto message = isUp ? WM_KEYUP : WM_KEYDOWN;
	PostMessageW(WindowHWND, message, KeyMapDict[(INT16)wParam], lParam);
	return 1;
}