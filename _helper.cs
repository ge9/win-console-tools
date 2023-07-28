using System;
using System.Runtime.InteropServices;
class Helper
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
    public static IntPtr ExecuteCommand(string command, IntPtr inputPipe, IntPtr outputPipe)
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
}