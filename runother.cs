using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;


class Program
{
    static int Main(string[] args)
    {
        string commandLine = Helper.SeparateExecPath(Environment.CommandLine).Item2;
        
        string myName = System.Reflection.Assembly.GetExecutingAssembly().Location;
        myName = myName.Substring(0, myName.Length - 3) + 't' + myName[myName.Length - 2] + 't';

        string newCommand = File.ReadAllText(myName, Encoding.UTF8) + commandLine;

        return Helper.StdExecuteCommand(newCommand);
    }
    
}