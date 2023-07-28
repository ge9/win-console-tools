using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    static void Main(string[] args)
    {
        Helper.STARTUPINFO si = new Helper.STARTUPINFO();
        si.cb = Marshal.SizeOf(si);
        si.dwFlags = 0x00000001; // STARTF_USESHOWWINDOW
        si.wShowWindow = 1; // SW_SHOWNORMAL

        Helper.PROCESS_INFORMATION pi;
        string commandLine = Helper.SeparateExecPath(Environment.CommandLine).Item2;
        
        if (!Helper.CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, false, 0x00000010, // CREATE_NEW_CONSOLE
            IntPtr.Zero, null, ref si, out pi))
        {
            throw new System.ComponentModel.Win32Exception();
        }
        Helper.CloseHandle(pi.hProcess);
        Helper.CloseHandle(pi.hThread);
    }
}