using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    static void Main(string[] args)
    {
        string commandLine0 = Helper.SeparateExecPath(Environment.CommandLine).Item2;
        Tuple<string, string> commandLine1 = Helper.SeparateExecPath(commandLine0);
        string file = commandLine1.Item1;
        string parameters = commandLine1.Item2;

        Helper.SHELLEXECUTEINFO shExInfo = new Helper.SHELLEXECUTEINFO();
        shExInfo.cbSize = Marshal.SizeOf(shExInfo);
        shExInfo.fMask = 0x00000040; // SEE_MASK_NOCLOSEPROCESS
        shExInfo.hwnd = IntPtr.Zero;
        shExInfo.lpVerb = "runas";
        shExInfo.lpFile = file;
        shExInfo.lpParameters = parameters;
        shExInfo.lpDirectory = null;
        shExInfo.nShow = 5; // SW_SHOW
        shExInfo.hInstApp = IntPtr.Zero;

        if (Helper.ShellExecuteEx(ref shExInfo))
        {
            Helper.WaitForSingleObject(shExInfo.hProcess, 0xFFFFFFFF); // INFINITE
            Helper.CloseHandle(shExInfo.hProcess);  // Close the handle to the process
        }
    }
}