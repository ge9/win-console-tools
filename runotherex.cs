using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static int Main(string[] args)
    {
        return Executor.StdExecuteCommand(Runotherex.getNewCmd());
    }
    
}