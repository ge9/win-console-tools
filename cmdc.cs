using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;


class Program
{
    static int Main(string[] args)
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

        string commandLine = "cmd /c \"set "+varName+"=&"+Helper.SeparateExecPath(Environment.CommandLine).Item2.Replace("%","%"+varName+"%")+"\"";

        return Helper.StdExecuteCommand(commandLine);
    }
    
}