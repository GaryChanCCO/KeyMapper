using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32;
using winmdroot = global::Windows.Win32;

namespace KeyMapper;

public static class Program
{
    private static readonly Mutex mutex = new(true, "960BC6A1-223B-4A5D-A79A-E5CF644E6E35");

    private static readonly FreeLibrarySafeHandle hookInjectDLL = PInvoke.LoadLibrary(KeyMapperHook.dllName);
    private static readonly ILoggerFactory loggerFactory;
    private static readonly ILogger logger;

    static Program()
    {
        Console.OutputEncoding = Console.InputEncoding = Encoding.UTF8;
        loggerFactory = LoggerFactory.Create(conf => conf.AddConsole());
        logger = loggerFactory.CreateLogger("");
        if (!mutex.WaitOne(TimeSpan.Zero, true))
        {
            logger.LogInformation("Another instance of this application is already running.");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Environment.Exit(0);
        }
    }


    public static void Main(string[] args)
    {
        try
        {
            var windowHWnd = winmdroot.Foundation.HWND.Null;
            PInvoke.EnumWindows(new winmdroot.UI.WindowsAndMessaging.WNDENUMPROC((hWnd, _) =>
            {
                var charArr = new char[2048];
                unsafe
                {
                    fixed (char* charArrPtr = charArr)
                    {
                        PInvoke.GetWindowText(hWnd, charArrPtr, charArr.Length);
                    }
                }
                var title = new string(charArr);
                if (title.Contains("Notepad++"))
                {
                    windowHWnd = hWnd;
                    logger.LogInformation(title);
                }
                return true;
            }), 0);

            // if (windowHWnd.IsNull) throw new Exception($"Cannot find window which match regex ({windowNameRx})");

            uint threadId;
            uint pid = 0;
            unsafe
            {
                threadId = PInvoke.GetWindowThreadProcessId(windowHWnd, &pid);
            }
            if (threadId == 0) throw new Win32Exception();

            //var processHandle = PInvoke.OpenProcess(winmdroot.System.Threading.PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION
            //                                               | winmdroot.System.Threading.PROCESS_ACCESS_RIGHTS.PROCESS_VM_READ
            //                                               | winmdroot.System.Threading.PROCESS_ACCESS_RIGHTS.PROCESS_SYNCHRONIZE,
            //                                               false,
            //                                               pid);
            //{
            //    var processName = PInvoke.GetModuleBaseName(processHandle, IntPtr.Zero);
            //    if (!processNameRx.IsMatch(processName))
            //        throw new Exception($"Process with PID {pid} do not match the regex {processNameRx}");
            //    Console.WriteLine($"Target process id: {pid}");
            //    Console.WriteLine($"Target process name: {processName}");
            //}

            var hookHandle = PInvoke.SetWindowsHookEx(winmdroot.UI.WindowsAndMessaging.WINDOWS_HOOK_ID.WH_KEYBOARD,
                                                      KeyMapperHook.KeyboardProc,
                                                      hookInjectDLL,
                                                      threadId);

            while (true) Task.Delay(1000).Wait();
            //PInvoke.UnhookWindowsHookEx(hookHandle.DangerousGetHandle());
        }
        catch (Exception e)
        {
            logger.LogError($"{e.GetType().Name}:{e.Message}");
        }
        finally
        {
            mutex.ReleaseMutex();
            Thread.Sleep(1000);
        }
    }
}

