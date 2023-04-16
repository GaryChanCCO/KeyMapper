using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace KeyMapper.Native
{
    public static class KeyMapperHookInject
    {
        [DllImport(nameof(KeyMapperHookInject))]
        public extern static void WriteGloBalVariables();

        [DllImport(nameof(KeyMapperHookInject))]
        public extern static void Set_WindowHWND(IntPtr ptr);

        [DllImport(nameof(KeyMapperHookInject))]
        private extern static void Set_KeyMapDict(short[] originalKeys, short[] targetKeys, uint length);
        public static void Set_KeyMapDict2(IDictionary<short, short> dict)
        {
            var originalKeys = new List<short>(dict.Count);
            var targetKeys = new List<short>(dict.Count);
            foreach(var kvp in dict)
            {
                originalKeys.Add(kvp.Key);
                targetKeys.Add(kvp.Value);
            }
            Set_KeyMapDict(originalKeys.ToArray(), targetKeys.ToArray(), (uint)dict.Count);
        }

        [DllImport(nameof(KeyMapperHookInject))]
        public extern static int KeyboardProc(short code, int wParam, int lParam);
    }
}
