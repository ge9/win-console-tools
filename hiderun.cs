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
        si.wShowWindow = 0; // SW_HIDE

        Helper.PROCESS_INFORMATION pi;
        string commandLine = Helper.SeparateExecPath(Environment.CommandLine).Item2;
        
        if (!Helper.CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, false, 0x08000000, // CREATE_NO_WINDOW
            IntPtr.Zero, null, ref si, out pi))
        {
            throw new System.ComponentModel.Win32Exception();
        }
        Helper.CloseHandle(pi.hProcess);
        Helper.CloseHandle(pi.hThread);
    }
}