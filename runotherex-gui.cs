using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;


class Program
{
    static int Main(string[] args)
    {
        string commandLine = Helper.SeparateExecPath(Environment.CommandLine).Item2;
        
        string myName = System.Reflection.Assembly.GetExecutingAssembly().Location;
        myName = myName.Substring(0, myName.Length - 3) + 't' + myName[myName.Length - 2] + 't';

        string[] split_cmd = File.ReadAllText(myName, Encoding.UTF8).Split(new[] { "\r\n", "\r", "\n" }, 2, StringSplitOptions.None);
        string newCommand = Regex.Replace(split_cmd[1], Regex.Escape(split_cmd[0])+"(.)", new MatchEvaluator(match => {
            switch (match.Groups[1].Value)
            {
                case "a": return commandLine.Replace("&", "&&");
                case "b": return commandLine.Replace("|", "||");
                case "c": return commandLine;
                case "p": return commandLine.Replace("'", "''").Replace("‘", "‘‘").Replace("’", "’’").Replace("‚", "‚‚").Replace("‛", "‛‛");
                case "d": return System.IO.Path.GetDirectoryName(myName);
                default: throw new System.ComponentModel.Win32Exception("illegal character after prefix:"+match.Groups[1].Value);
            }
        }));       

        Helper.STARTUPINFO si = new Helper.STARTUPINFO();
        si.cb = Marshal.SizeOf(si);
        Helper.PROCESS_INFORMATION pi = new Helper.PROCESS_INFORMATION();

        if (!Helper.CreateProcess(null, newCommand, IntPtr.Zero, IntPtr.Zero, true, 0, IntPtr.Zero, null, ref si, out pi))
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