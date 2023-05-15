// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

using namespace std;

void WriteLine(std::string message) {
	message = message + "\n";
	WriteConsoleA(GetStdHandle(STD_OUTPUT_HANDLE), message.c_str(), message.length(), NULL, NULL);
}

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


typedef void (*ProcessKeyboardProcArgs)(INT32 code, WPARAM wParam, LPARAM lParam);

ProcessKeyboardProcArgs processKeyboardProcArgs = [](INT32 code, WPARAM wParam, LPARAM lParam) -> void {};
EXTERN_C __declspec(dllexport) void SetProcessKeyboardProcArgs(ProcessKeyboardProcArgs f) {
	processKeyboardProcArgs = f;
}

EXTERN_C  __declspec(dllexport) LRESULT CALLBACK KeyboardProc(INT32 code, WPARAM wParam, LPARAM lParam) {
	return CallNextHookEx(NULL, code, wParam, lParam);
}