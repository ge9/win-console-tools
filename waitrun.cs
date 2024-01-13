using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;


class Program
{
    static int Main(string[] args)
    {
        string commandLine = Helper.SeparateExecPath(Environment.CommandLine).Item2;

        int exitCode = Helper.StdExecuteCommand(commandLine);
        
        Console.WriteLine("waitrun: press some key to exit...");
	    Console.ReadKey();
        return exitCode;
    }
    
}