using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

public class Program
{
    static int Main(string[] args)
    {
        return cmdc(Helper.SeparateExecPath(Environment.CommandLine).Item2);
    }
    public static int cmdc(string commandLine0)
    {
        int index = 1;
        string varName;
        while (true)
        {
            varName = "VAR" + index;
            if (Environment.GetEnvironmentVariable(varName) == null) break;
            index++;
        }
        Environment.SetEnvironmentVariable(varName,"%");

        string commandLine = "cmd /c \"set "+varName+"=&"+commandLine0.Replace("%","%"+varName+"%")+"\"";

        return Helper.StdExecuteCommand(commandLine);
    }
    
}