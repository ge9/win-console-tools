using System;
using System.Runtime.InteropServices;

public class Helper
{
    public static readonly int STD_INPUT_HANDLE = -10;
    public static readonly int STD_OUTPUT_HANDLE = -11;
    public static readonly int STD_ERROR_HANDLE = -12;
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask, HANDLE_FLAGS dwFlags);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);

    public delegate bool ConsoleCtrlDelegate(uint ctrlType);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool CreateProcess(
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        [In] ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation
    );
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        ref SECURITY_ATTRIBUTES lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);
    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public int bInheritHandle;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct STARTUPINFO
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }
    [Flags]
    public enum HANDLE_FLAGS : uint
    {
        None = 0,
        INHERIT = 1,
        PROTECT_FROM_CLOSE = 2
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

    [DllImport("kernel32.dll")]
    public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
    [DllImport("kernel32.dll")]
    public static extern bool WaitForMultipleObjects(uint nCount, IntPtr[] lpHandles,
       bool bWaitAll, uint dwMilliseconds);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SHELLEXECUTEINFO
    {
        public int cbSize;
        public uint fMask;
        public IntPtr hwnd;
        public string lpVerb;
        public string lpFile;
        public string lpParameters;
        public string lpDirectory;
        public int nShow;
        public IntPtr hInstApp;
        public IntPtr lpIDList;
        public string lpClass;
        public IntPtr hkeyClass;
        public uint dwHotKey;
        public IntPtr hIcon;
        public IntPtr hProcess;
    }

    public static Tuple<string, string> SeparateExecPath(string s)
    {
        string e;
        if (s.StartsWith("\""))
        {
            e = s.Substring(0, s.IndexOf("\"", 1) + 1);
            s = s.Substring(s.IndexOf("\"", 1) + 1);
            
        }
        else
        {
            int spaceIndex = s.IndexOf(" ");
            int tabIndex = s.IndexOf("\t");

            // If neither space nor tab was found, just return the string as is
            if (spaceIndex == -1 && tabIndex == -1)
                return Tuple.Create(s, "");

            int firstWhitespaceIndex = (spaceIndex == -1) ? tabIndex
                                      : (tabIndex == -1) ? spaceIndex
                                      : (spaceIndex < tabIndex ? spaceIndex : tabIndex);

            e = s.Substring(0, firstWhitespaceIndex);
            s = s.Substring(firstWhitespaceIndex);
        }

        return Tuple.Create(e, s.TrimStart(new[] { ' ', '\t' }));
    }
    //handlerなどを設定しないシンプルな実行。結局piperun系でしか使われていない。
    public static IntPtr SimpleExecuteCommand(string command, IntPtr inputPipe, IntPtr outputPipe)
    {
        var siStartInfo = new Helper.STARTUPINFO();
        var piProcInfo = new Helper.PROCESS_INFORMATION();

        siStartInfo.cb = Marshal.SizeOf(siStartInfo);
        siStartInfo.hStdError = Helper.GetStdHandle(Helper.STD_ERROR_HANDLE);
        siStartInfo.hStdOutput = outputPipe;
        siStartInfo.hStdInput = inputPipe;
        siStartInfo.dwFlags |= 0x00000100;  // STARTF_USESTDHANDLES flag

        if (!Helper.CreateProcess(null, command, IntPtr.Zero, IntPtr.Zero, true, 0, IntPtr.Zero, null, ref siStartInfo, out piProcInfo)){
            throw new Exception("CreateProcess failed.");
        }
        Helper.CloseHandle(piProcInfo.hThread);
        return piProcInfo.hProcess;
    }
    //handlerを使用し、標準入出力を使用して実行する。
    public static int StdExecuteCommand(string commandLine){
        // Prepare the STARTUPINFO structure
        Helper.STARTUPINFO si = new Helper.STARTUPINFO();
        si.cb = Marshal.SizeOf(si);
        si.dwFlags = 0x00000100; // STARTF_USESTDHANDLES
        si.hStdInput = Helper.GetStdHandle(Helper.STD_INPUT_HANDLE);
        si.hStdOutput = Helper.GetStdHandle(Helper.STD_OUTPUT_HANDLE);
        si.hStdError = Helper.GetStdHandle(Helper.STD_ERROR_HANDLE);
        
        Helper.PROCESS_INFORMATION pi;

        if (!Helper.CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, true, 0x00000200, // CREATE_NEW_PROCESS_GROUP
                                    IntPtr.Zero, null, ref si, out pi)){
            throw new System.ComponentModel.Win32Exception();
        }

        Helper.SetConsoleCtrlHandler(new Helper.ConsoleCtrlDelegate(new Handler((uint)pi.dwProcessId).HandlerRoutine), true);
        uint r = Helper.WaitForSingleObject(pi.hProcess, uint.MaxValue); // INFINITE
        if (r != 0) {// WAIT_OBJECT_0
            Console.WriteLine("Wait failed; exit code {0}", r); return -1;
        }
        
        uint exitCode;
        // Get the exit code
        if (!Helper.GetExitCodeProcess(pi.hProcess, out exitCode)){
            Console.WriteLine("GetExitCodeProcess failed"); return -1;
        }
        // Close the handles
        Helper.CloseHandle(pi.hProcess);
        Helper.CloseHandle(pi.hThread);

        return (int)exitCode;
    }
}

class Handler
{
    private uint pipid;
    public Handler(uint pipid) {
        this.pipid = pipid;
    }
    public bool HandlerRoutine(uint dwCtrlType)
    {
        if (dwCtrlType == 0)
        {
            Helper.GenerateConsoleCtrlEvent(1, pipid);
            return true;
        }
        return false;
    }
}