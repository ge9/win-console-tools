using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.Generic;

class Program
{
    static List<string> SplitCommands(string input)
    {
        var commands = new List<string>();
        var currentCommand = "";

        for (int i = 0; i < input.Length; ++i){
            if (input[i] == '|'){
                if (i + 1 < input.Length && input[i + 1] == '|') // Next character is also a pipe
                {
                    currentCommand += '|'; // Treat it as an escaped pipe
                    ++i; // Skip the next character
                }
                else{ // It's a separator
                    commands.Add(currentCommand); // Save current command
                    currentCommand = ""; // Prepare for the next one
                }
            }
            else{
                currentCommand += input[i];
            }
        }
        commands.Add(currentCommand); // The last command
        // Trim leading spaces from commands
        for (int i = 0; i < commands.Count; i++)
        {
            commands[i] = Regex.Replace(commands[i], @"^\s+", "");
        }
        return commands;
    }
    
    static void Main(string[] args)
    {
        var commands = SplitCommands(Helper.SeparateExecPath(Environment.CommandLine).Item2);
        List<IntPtr> processHandles = new List<IntPtr>();
        Helper.SECURITY_ATTRIBUTES sa = new Helper.SECURITY_ATTRIBUTES();
            sa.nLength = Marshal.SizeOf(sa);
            sa.bInheritHandle = 1;

        IntPtr prevPipe = Helper.GetStdHandle(Helper.STD_INPUT_HANDLE);
        for (int i = 0; i < commands.Count; ++i)
        {
            IntPtr pipeIn = IntPtr.Zero;
            IntPtr pipeOut = Helper.GetStdHandle(Helper.STD_OUTPUT_HANDLE);

            if (i < commands.Count - 1)
            {
                IntPtr readPipe, writePipe;
                Helper.CreatePipe(out readPipe, out writePipe, ref sa, 0);
                pipeIn = readPipe;
                pipeOut = writePipe;
            }
            processHandles.Add(Helper.SimpleExecuteCommand(commands[i], prevPipe, pipeOut));

            if (i > 0) Helper.CloseHandle(prevPipe);
            if (i < commands.Count - 1) Helper.CloseHandle(pipeOut);
            prevPipe = pipeIn;
        }
        // Wait for all processes to finish
        Helper.WaitForMultipleObjects((uint)processHandles.Count, processHandles.ToArray(), true, uint.MaxValue);

        foreach (var handle in processHandles) Helper.CloseHandle(handle);
    }
}