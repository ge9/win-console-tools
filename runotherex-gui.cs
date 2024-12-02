using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;


class Program
{
    static int Main(string[] args)
    {      

        Helper.STARTUPINFO si = new Helper.STARTUPINFO();
        si.cb = Marshal.SizeOf(si);
        Helper.PROCESS_INFORMATION pi = new Helper.PROCESS_INFORMATION();

        if (!Helper.CreateProcess(null, Runotherex.getNewCmd(), IntPtr.Zero, IntPtr.Zero, true, 0, IntPtr.Zero, null, ref si, out pi))
            return -1;

        uint r = Helper.WaitForSingleObject(pi.hProcess, uint.MaxValue); // INFINITE
        if (r != 0) {// WAIT_OBJECT_0
            Console.WriteLine("Wait failed; exit code {0}", r); return -1;
        }
        
        // Get the exit code
        uint exitCode;
        if (!Helper.GetExitCodeProcess(pi.hProcess, out exitCode)){
            Console.WriteLine("GetExitCodeProcess failed"); return -1;
        }
        // C#ではマネージドメモリを使用しているので、明示的にdelete[]を行う必要はありません。

        Helper.CloseHandle(pi.hProcess);
        Helper.CloseHandle(pi.hThread);

        return (int)exitCode;
    }
    
}