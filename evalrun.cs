using System;
using System.Runtime.InteropServices;
using System.Text;



class Program
{
    static int Main(string[] args)
    {
        string outstr;
        
        {
            string commandLine = Helper.SeparateExecPath(Environment.CommandLine).Item2;
            Helper.SECURITY_ATTRIBUTES sa = new Helper.SECURITY_ATTRIBUTES();
            sa.nLength = Marshal.SizeOf(sa);
            sa.bInheritHandle = 1;

            IntPtr hRead, hWrite;
            if (!Helper.CreatePipe(out hRead, out hWrite, ref sa, 0))
                throw new System.ComponentModel.Win32Exception();
            if (!Helper.SetHandleInformation(hRead, Helper.HANDLE_FLAGS.INHERIT, 0))
                throw new System.ComponentModel.Win32Exception();

            Helper.STARTUPINFO si0 = new Helper.STARTUPINFO();
            si0.cb = Marshal.SizeOf(si0);
            si0.dwFlags = 0x00000100;  // STARTF_USESTDHANDLES flag
            si0.hStdOutput = hWrite;
            si0.hStdError = Helper.GetStdHandle(Helper.STD_ERROR_HANDLE);

            Helper.PROCESS_INFORMATION pi0;
            if (!Helper.CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, true, 0, IntPtr.Zero, null, ref si0, out pi0))
                throw new System.ComponentModel.Win32Exception();

            Helper.CloseHandle(hWrite);

            byte[] buffer = new byte[4096];
            uint bytesRead;
            StringBuilder output = new StringBuilder();
            // 標準出力からデータを読み取る
            while (Helper.ReadFile(hRead, buffer, 4096, out bytesRead, IntPtr.Zero) && bytesRead > 0)
                output.Append(Encoding.UTF8.GetString(buffer, 0, (int)bytesRead));

            outstr = output.ToString();

            Helper.CloseHandle(pi0.hProcess);
            Helper.CloseHandle(pi0.hThread);
            Helper.CloseHandle(hRead);
        }
        return Helper.StdExecuteCommand(outstr);
    }
    
}