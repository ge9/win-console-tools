//deprecated: doesn't work correctly with Unicode characters
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine(Helper.SeparateExecPath(Environment.CommandLine).Item2);
    }
}