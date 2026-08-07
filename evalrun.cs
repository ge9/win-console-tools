using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;



public class Program
{
    static int Main(string[] args)
    {
        return evalrun(Helper.SeparateExecPath(Environment.CommandLine).Item2);
    }
    public static int evalrun(string commandLine)
    {
        string outstr;
        {
            Helper.SECURITY_ATTRIBUTES sa = new Helper.SECURITY_ATTRIBUTES();
            sa.nLength = Marshal.SizeOf(sa);
            sa.bInheritHandle = 1;

            IntPtr hRead, hWrite;
            if (!Helper.CreatePipe(out hRead, out hWrite, ref sa, 0))
                throw new System.ComponentModel.Win32Exception();

            if (!Helper.SetHandleInformation(hRead, Helper.HANDLE_FLAGS.INHERIT, 0))
            {
                Helper.CloseHandle(hRead);
                Helper.CloseHandle(hWrite);
                throw new System.ComponentModel.Win32Exception();
            }

            StringBuilder output = new StringBuilder();

            try
            {
                int result = Executor.ExecuteCore(
                    commandLine,
                    0, // creationFlags (0)
                    delegate(Helper.STARTUPINFO si) {
                        si.dwFlags = 0x00000100; // STARTF_USESTDHANDLES
                        si.hStdOutput = hWrite;
                        si.hStdError = Helper.GetStdHandle(Helper.STD_ERROR_HANDLE);
                    },
                    delegate(Helper.PROCESS_INFORMATION pi) {
                        Helper.CloseHandle(hWrite);
                        hWrite = IntPtr.Zero;

                        byte[] buffer = new byte[4096];
                        uint bytesRead;
                        while (Helper.ReadFile(hRead, buffer, 4096, out bytesRead, IntPtr.Zero) && bytesRead > 0)
                        {
                            output.Append(Encoding.UTF8.GetString(buffer, 0, (int)bytesRead));
                        }
                    }
                );

                if (result != 0)
                {
                    return result;
                }
            }
            finally
            {
                if (hWrite != IntPtr.Zero) Helper.CloseHandle(hWrite);
                if (hRead != IntPtr.Zero) Helper.CloseHandle(hRead);
            }

            outstr = output.ToString();
        }

        return Executor.StdExecuteCommand(outstr);
    }
    
}