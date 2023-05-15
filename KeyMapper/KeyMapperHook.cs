using System;
using System.Runtime.InteropServices;
using winmdroot = global::Windows.Win32;

namespace KeyMapper
{
    internal static class KeyMapperHook
    {
        public const string dllName = "KeyMapperHook.dll";

        internal delegate void ProcessKeyboardProcArgs(int code, winmdroot.Foundation.WPARAM wParam, winmdroot.Foundation.LPARAM lParam);

        [DllImport(dllName, ExactSpelling = true)]
        internal extern static void SetProcessKeyboardProcArgs(ProcessKeyboardProcArgs processKeyboardProcArgs);

        [DllImport(dllName, ExactSpelling = true)]
        internal extern static winmdroot.Foundation.LRESULT KeyboardProc(int code, winmdroot.Foundation.WPARAM wParam, winmdroot.Foundation.LPARAM lParam);

    }
}
