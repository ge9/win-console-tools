using System;
using System.Runtime.InteropServices;

public class Executor
{
    public delegate void ConfigureStartupInfoDelegate(Helper.STARTUPINFO si);
    public delegate void PreWaitActionDelegate(Helper.PROCESS_INFORMATION pi);
    public static int ExecuteCore(
        string commandLine,
        uint creationFlags,
        ConfigureStartupInfoDelegate configureStartupInfo,
        PreWaitActionDelegate preWaitAction)
    {
        Helper.STARTUPINFO si = new Helper.STARTUPINFO();
        si.cb = Marshal.SizeOf(si);

        if (configureStartupInfo != null)
        {
            configureStartupInfo(si);
        }

        Helper.PROCESS_INFORMATION pi;
        if (!Helper.CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, true, creationFlags,
                                    IntPtr.Zero, null, ref si, out pi))
        {
            Console.Error.WriteLine("CreateProcess failed");
            return -1;
        }

        try
        {
            if (preWaitAction != null)
            {
                preWaitAction(pi);
            }

            uint r = Helper.WaitForSingleObject(pi.hProcess, uint.MaxValue); // INFINITE
            if (r != 0) // WAIT_OBJECT_0
            {
                Console.Error.WriteLine("Wait failed; exit code {0}", r);
                return -1;
            }

            uint exitCode;
            if (!Helper.GetExitCodeProcess(pi.hProcess, out exitCode))
            {
                Console.Error.WriteLine("GetExitCodeProcess failed");
                return -1;
            }

            return (int)exitCode;
        }
        finally
        {
            if (pi.hProcess != IntPtr.Zero) Helper.CloseHandle(pi.hProcess);
            if (pi.hThread != IntPtr.Zero) Helper.CloseHandle(pi.hThread);
        }
    }
    public static int StdExecuteCommand(string commandLine){
        return ExecuteCore(
            commandLine,
            0x00000200, // CREATE_NEW_PROCESS_GROUP
            delegate(Helper.STARTUPINFO si) {
                si.dwFlags = 0x00000100; // STARTF_USESTDHANDLES
                si.hStdInput = Helper.GetStdHandle(Helper.STD_INPUT_HANDLE);
                si.hStdOutput = Helper.GetStdHandle(Helper.STD_OUTPUT_HANDLE);
                si.hStdError = Helper.GetStdHandle(Helper.STD_ERROR_HANDLE);
            },
            delegate(Helper.PROCESS_INFORMATION pi) {
                Helper.SetConsoleCtrlHandler(new Helper.ConsoleCtrlDelegate(new Handler((uint)pi.dwProcessId).HandlerRoutine), true);
            }
        );
    }
}
