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
            if (input[i] == '|') {
            if (i + 1 < input.Length && input[i + 1] == '|') { // Next character is also a pipe
                currentCommand += '|'; // Treat it as an escaped pipe
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
    static void Main(string[] args)
    {
        var commands = SplitCommands(Helper.SeparateExecPath(Environment.CommandLine).Item2);
        IntPtr[] processHandles = new IntPtr[2];
        {
            Helper.SECURITY_ATTRIBUTES sa = new Helper.SECURITY_ATTRIBUTES();
            sa.nLength = Marshal.SizeOf(sa);
            sa.bInheritHandle = 1;

            IntPtr readPipe, writePipe;
            Helper.CreatePipe(out readPipe, out writePipe, ref sa, 0);
            processHandles[0] = Helper.ExecuteCommand(commands.Item1, Helper.GetStdHandle(Helper.STD_INPUT_HANDLE), writePipe);
            Helper.CloseHandle(writePipe);

            processHandles[1] = Helper.ExecuteCommand(commands.Item2, readPipe, Helper.GetStdHandle(Helper.STD_OUTPUT_HANDLE));
            Helper.CloseHandle(readPipe);
        }
        // Wait for all processes to finish
        Helper.WaitForMultipleObjects(2, processHandles, true, uint.MaxValue); // INFINITE = 0xFFFFFFFF
        Helper.CloseHandle(processHandles[0]);
        Helper.CloseHandle(processHandles[1]);
    }
}