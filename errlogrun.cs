using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

class Program
{
    static int Main(string[] args)
    {
        string commandLine0 = Helper.SeparateExecPath(Environment.CommandLine).Item2;
        Tuple<string, string> cmd_temp = Helper.SeparateExecPath(commandLine0);
        string commandLine = cmd_temp.Item2;
        string logFile = cmd_temp.Item1.Trim('"');
        Helper.SECURITY_ATTRIBUTES sa = new Helper.SECURITY_ATTRIBUTES();
        sa.nLength = Marshal.SizeOf(sa);
        sa.bInheritHandle = 1;
        IntPtr hErrorFile = Helper.CreateFile(logFile, 0x40000000, 0, ref sa, 2, 0, IntPtr.Zero); //GENERIC_WRITE, CREATE_ALWAYS
        IntPtr hReadPipe, hWritePipe;
        Helper.CreatePipe(out hReadPipe, out hWritePipe, ref sa, 0);

        // Prepare the STARTUPINFO structure
        Helper.STARTUPINFO si = new Helper.STARTUPINFO();
        si.cb = Marshal.SizeOf(si);
        si.dwFlags = 0x00000100; // STARTF_USESTDHANDLES
        si.hStdInput = Helper.GetStdHandle(Helper.STD_INPUT_HANDLE);
        si.hStdOutput = Helper.GetStdHandle(Helper.STD_OUTPUT_HANDLE);
        si.hStdError = hWritePipe;

        Helper.PROCESS_INFORMATION pi;

        if (!Helper.CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, true, 0x00000200, // CREATE_NEW_PROCESS_GROUP
                                    IntPtr.Zero, null, ref si, out pi)){
            throw new System.ComponentModel.Win32Exception();
        }

        Helper.SetConsoleCtrlHandler(new Helper.ConsoleCtrlDelegate(new Handler((uint)pi.dwProcessId).HandlerRoutine), true);
        Helper.CloseHandle(hWritePipe);

        byte[] buffer = new byte[4096];
        uint bytesRead;
        uint bytesWritten;
        while (Helper.ReadFile(hReadPipe, buffer, (uint)buffer.Length, out bytesRead, IntPtr.Zero) && bytesRead > 0)
        {
            // ログファイルに書き込む
            Helper.WriteFile(hErrorFile, buffer, bytesRead, out bytesWritten, IntPtr.Zero);
            
            // 自身の標準エラー出力にも書き込む
            Helper.WriteFile(Helper.GetStdHandle(Helper.STD_ERROR_HANDLE), buffer, bytesRead, out bytesWritten, IntPtr.Zero);
        }

        uint r = Helper.WaitForSingleObject(pi.hProcess, uint.MaxValue); // INFINITE
        if (r != 0) {// WAIT_OBJECT_0
            Console.WriteLine("Wait failed; exit code {0}", r); return -1;
        }
        
        // Get the exit code
        uint exitCode;
        if (!Helper.GetExitCodeProcess(pi.hProcess, out exitCode)){
            Console.WriteLine("GetExitCodeProcess failed"); return -1;
        }
        
        // Close the handles
        Helper.CloseHandle(pi.hProcess);
        Helper.CloseHandle(pi.hThread);

        // プロセスが終了したら標準エラーハンドルとログファイルハンドルを閉じる
        Helper.CloseHandle(hErrorFile);
        Helper.CloseHandle(hReadPipe);
        return (int)exitCode;
    }
    
}