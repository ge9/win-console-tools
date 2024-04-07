using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

public class Program
{
    static int Main(string[] args)
    {
        return waitrun(Helper.SeparateExecPath(Environment.CommandLine).Item2);
    }
    public static int waitrun(string commandLine)
    {
        int exitCode = Helper.StdExecuteCommand(commandLine);
        
        Console.WriteLine("waitrun: press some key to exit...");
	    Console.ReadKey();
        return exitCode;
    }
    
}