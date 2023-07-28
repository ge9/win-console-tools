using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(Helper.SeparateExecPath(Environment.CommandLine).Item2);
    }
}