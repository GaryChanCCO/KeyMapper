using KeyMapper.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using PInvoke;
using System.Text.Json;

namespace KeyMapper
{
    public static class Program
    {
        private static readonly IntPtr hookInjectDLL = Kernel32.LoadLibrary($"{nameof(KeyMapperHookInject)}.dll").DangerousGetHandle();
        private static readonly AppSetting appSetting;

        static Program()
        {
            Console.OutputEncoding = Encoding.Unicode;

            appSetting = JsonSerializer.Deserialize<AppSetting>(File.ReadAllText($"{nameof(Configuration)}.json"))!.EnsureValidSetting();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                IntPtr? windowHandle = null;
                User32.Enum
                Win32Functions.EnumChildWindows(IntPtr.Zero, new Win32Functions.EnumChildProc((handle, _) =>
                {
                    try
                    {
                        var title = Win32Functions.GetWindowText2(handle);
                        if(windowNameRx.IsMatch(title))
                        {
                            windowHandle = handle;
                            Console.WriteLine($"Target window: {title}");
                            return false;
                        }
                    }
                    catch(Win32Exception) { }
                    return true;
                }), 0);

                if(windowHandle == null)
                    throw new Exception($"Cannot find window which match regex ({windowNameRx})");


                var threadId = Win32Functions.GetWindowThreadProcessId(windowHandle.Value, out uint pid);
                var processHandle = Win32Functions.OpenProcess(Win32Functions.ProcessAccessRight.PROCESS_QUERY_INFORMATION
                                                               | Win32Functions.ProcessAccessRight.PROCESS_VM_READ
                                                               | Win32Functions.ProcessAccessRight.SYNCHRONIZE,
                                                               false,
                                                               pid);
                {
                    var processName = Win32Functions.GetModuleBaseName2(processHandle, IntPtr.Zero);
                    if(!processNameRx.IsMatch(processName))
                        throw new Exception($"Process with PID {pid} do not match the regex {processNameRx}");
                    Console.WriteLine($"Target process id: {pid}");
                    Console.WriteLine($"Target process name: {processName}");
                }

                KeyMapperHookInject.Set_WindowHWND(windowHandle.Value);
                KeyMapperHookInject.Set_KeyMapDict2(conf.KeyMapDict);
                KeyMapperHookInject.WriteGloBalVariables();

                Win32Functions.SetWindowsHookEx2(
                    new Win32Functions.KeyboardProc(KeyMapperHookInject.KeyboardProc),
                    hookInjectDLL,
                    threadId);

                while(Win32Functions.IsWindow(windowHandle.Value))
                {
                    Task.Delay(1000).Wait();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"{e.GetType()}: {e.Message}");
                if(e.InnerException != null)
                    Console.WriteLine($"    {e.InnerException.GetType()}: {e.InnerException.Message}");
                Console.WriteLine(e.StackTrace);
            }
            Win32Functions.System("pause");
        }
    }
}
