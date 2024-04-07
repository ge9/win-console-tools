//experimental: stopping the dead loop is difficult (Ctrl+C doesn't work)
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class Program
{
    static int Main(string[] args)
    {
        return looprun(Helper.SeparateExecPath(Environment.CommandLine).Item2);
    }
    public static int looprun(string commandLine)
    {
        while (true){
            Helper.StdExecuteCommand(commandLine);
        }
    }
}