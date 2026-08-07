using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

public class Program
{
    static int Main(string[] args)
    {
        return errlogrun(Helper.SeparateExecPath(Environment.CommandLine).Item2);
    }
    public static int errlogrun(string commandLine0)
    {
        Tuple<string, string> cmd_temp = Helper.SeparateExecPath(commandLine0);
        string commandLine = cmd_temp.Item2;
        string logFile = cmd_temp.Item1.Trim('"');

        Helper.SECURITY_ATTRIBUTES sa = new Helper.SECURITY_ATTRIBUTES();
        sa.nLength = Marshal.SizeOf(sa);
        sa.bInheritHandle = 1;

        IntPtr hErrorFile = Helper.CreateFile(logFile, 0x40000000, 0, ref sa, 2, 0, IntPtr.Zero);
        IntPtr hReadPipe, hWritePipe;
        Helper.CreatePipe(out hReadPipe, out hWritePipe, ref sa, 0);

        try
        {
            return Executor.ExecuteCore(
                commandLine,
                0x00000200, // CREATE_NEW_PROCESS_GROUP
                delegate(Helper.STARTUPINFO si) {
                    si.dwFlags = 0x00000100;
                    si.hStdInput = Helper.GetStdHandle(Helper.STD_INPUT_HANDLE);
                    si.hStdOutput = Helper.GetStdHandle(Helper.STD_OUTPUT_HANDLE);
                    si.hStdError = hWritePipe;
                },
                delegate(Helper.PROCESS_INFORMATION pi) {
                    Helper.SetConsoleCtrlHandler(new Helper.ConsoleCtrlDelegate(new Handler((uint)pi.dwProcessId).HandlerRoutine), true);
                    Helper.CloseHandle(hWritePipe);
                    hWritePipe = IntPtr.Zero;

                    byte[] buffer = new byte[4096];
                    uint bytesRead, bytesWritten;
                    while (Helper.ReadFile(hReadPipe, buffer, (uint)buffer.Length, out bytesRead, IntPtr.Zero) && bytesRead > 0)
                    {
                        Helper.WriteFile(hErrorFile, buffer, bytesRead, out bytesWritten, IntPtr.Zero);
                        Helper.WriteFile(Helper.GetStdHandle(Helper.STD_ERROR_HANDLE), buffer, bytesRead, out bytesWritten, IntPtr.Zero);
                    }
                }
            );
        }
        finally
        {
            if (hWritePipe != IntPtr.Zero) Helper.CloseHandle(hWritePipe);
            if (hReadPipe != IntPtr.Zero) Helper.CloseHandle(hReadPipe);
            if (hErrorFile != IntPtr.Zero) Helper.CloseHandle(hErrorFile);
        }
    }
    
}