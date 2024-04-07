using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class Program
{
    static Tuple<string, string> SplitCommands(string input)
    {
    string remaining = "";
    string currentCommand = "";

    for (int i = 0; i < input.Length; ++i){
            if (input[i] == '&') {
            if (i + 1 < input.Length && input[i + 1] == '&') { // Next character is also '&'
                currentCommand += '&'; // Treat it as an escaped '&'
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
        return andrun(Helper.SeparateExecPath(Environment.CommandLine).Item2);
    }
    public static int andrun(string commandLine)
    {
        var commands = SplitCommands(commandLine);
        Helper.StdExecuteCommand(commands.Item1);
        return Helper.StdExecuteCommand(commands.Item2);
    }
}