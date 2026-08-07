using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

public class Program
{
    static int Main(string[] args)
    {
        return fliprun(Helper.SeparateExecPath(Environment.CommandLine).Item2);
    }
    public static int fliprun(string commandLine)
    {
        return Executor.ExecuteCore(
            commandLine,
            0x00000200, // CREATE_NEW_PROCESS_GROUP
            delegate(Helper.STARTUPINFO si) {
                si.dwFlags = 0x00000100; // STARTF_USESTDHANDLES
                si.hStdInput = Helper.GetStdHandle(Helper.STD_INPUT_HANDLE);
                si.hStdOutput = Helper.GetStdHandle(Helper.STD_ERROR_HANDLE); // stdout to stderr
                si.hStdError = Helper.GetStdHandle(Helper.STD_OUTPUT_HANDLE);  // stderr to stdout
            },
            delegate(Helper.PROCESS_INFORMATION pi) {
                Helper.SetConsoleCtrlHandler(new Helper.ConsoleCtrlDelegate(new Handler((uint)pi.dwProcessId).HandlerRoutine), true);
            }
        );
    }
    
}