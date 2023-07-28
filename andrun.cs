using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

class Program
{
    static Tuple<string, string> SplitCommands(string input)
    {
    string remaining = "";
    string currentCommand = "";

    for (int i = 0; i < input.Length; ++i){
            if (input[i] == '&') {
            if (i + 1 < input.Length && input[i + 1] == '&') { // Next character is also a pipe
                currentCommand += '&'; // Treat it as an escaped pipe
                ++i; // Skip the next character
            }
            else { // It's a separator
                remaining = input.Substring(i + 1);
                break;
            }
        }
        else {
            currentCommand += input[i];
        }
    }

    // Trim leading spaces from remainings
    remaining = Regex.Replace(remaining, @"^\s+", "");
    return Tuple.Create(currentCommand,remaining);
    }
    static int Main(string[] args)
    {
        var commands = SplitCommands(Helper.SeparateExecPath(Environment.CommandLine).Item2);
        runInStd(commands.Item1);
        return runInStd(commands.Item2);
    }
    static int runInStd(string commandLine){
        // Prepare the STARTUPINFO structure
        Helper.STARTUPINFO si = new Helper.STARTUPINFO();
        si.cb = Marshal.SizeOf(si);
        si.dwFlags = 0x00000100; // STARTF_USESTDHANDLES
        si.hStdInput = Helper.GetStdHandle(Helper.STD_INPUT_HANDLE);
        si.hStdOutput = Helper.GetStdHandle(Helper.STD_OUTPUT_HANDLE);
        si.hStdError = Helper.GetStdHandle(Helper.STD_ERROR_HANDLE);
        
        Helper.PROCESS_INFORMATION pi;

        if (!Helper.CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, true, 0x00000200, // CREATE_NEW_PROCESS_GROUP
                                    IntPtr.Zero, null, ref si, out pi)){
            throw new System.ComponentModel.Win32Exception();
        }

        Helper.SetConsoleCtrlHandler(new Helper.ConsoleCtrlDelegate(new Handler((uint)pi.dwProcessId).HandlerRoutine), true);
        uint r = Helper.WaitForSingleObject(pi.hProcess, uint.MaxValue); // INFINITE
        if (r != 0) {// WAIT_OBJECT_0
            Console.WriteLine("Wait failed; exit code {0}", r); return -1;
        }
        
        uint exitCode;
        // Get the exit code
        if (!Helper.GetExitCodeProcess(pi.hProcess, out exitCode)){
            Console.WriteLine("GetExitCodeProcess failed"); return -1;
        }
        
        // Close the handles
        Helper.CloseHandle(pi.hProcess);
        Helper.CloseHandle(pi.hThread);

        return (int)exitCode;
    }
}