using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

public class Program
{
    static int Main(string[] args)
    {
        return hererun(Helper.SeparateExecPath(Environment.CommandLine).Item2);
    }
    public static int hererun(string commandLine0)
    {
        Tuple<string, string> command_tmp = Helper.SeparateExecPath(commandLine0);
        
        string commandLine = command_tmp.Item2.Replace(command_tmp.Item1, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

        return Helper.StdExecuteCommand(commandLine);
    }
    
}