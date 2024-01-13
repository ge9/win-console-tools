using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

class Program
{
    static int Main(string[] args)
    {
        string commandLine = Helper.SeparateExecPath(Environment.CommandLine).Item2;
        while (true){
            Helper.StdExecuteCommand(commandLine);
        }
    }
}