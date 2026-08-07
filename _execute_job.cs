using System;
using System.Runtime.InteropServices;

public class Executor
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

    [DllImport("kernel32.dll")]
    public static extern bool SetInformationJobObject(IntPtr hJob, int JobObjectInfoClass, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

    [DllImport("kernel32.dll")]
    public static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);
    [StructLayout(LayoutKind.Sequential)]
    public struct JOBOBJECT_BASIC_LIMIT_INFORMATION {
        public Int64 PerProcessUserTimeLimit;
        public Int64 PerJobUserTimeLimit;
        public UInt32 LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public UInt32 ActiveProcessLimit;
        public UIntPtr Affinity;
        public UInt32 PriorityClass;
        public UInt32 SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IO_COUNTERS {
        public UInt64 ReadOperationCount;
        public UInt64 WriteOperationCount;
        public UInt64 OtherOperationCount;
        public UInt64 ReadTransferCount;
        public UInt64 WriteTransferCount;
        public UInt64 OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }


    public delegate void ConfigureStartupInfoDelegate(Helper.STARTUPINFO si);
    public delegate void PreWaitActionDelegate(IntPtr hJob, Helper.PROCESS_INFORMATION pi);
    public static int ExecuteCore(
        string commandLine,
        uint creationFlags,
        ConfigureStartupInfoDelegate configureStartupInfo,
        PreWaitActionDelegate preWaitAction)
    {
        Helper.STARTUPINFO si = new Helper.STARTUPINFO();
        si.cb = Marshal.SizeOf(si);

        if (configureStartupInfo != null)
        {
            configureStartupInfo(si);
        }

        Helper.PROCESS_INFORMATION pi;
        if (!Helper.CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, true, creationFlags,
                                    IntPtr.Zero, null, ref si, out pi))
        {
            Console.Error.WriteLine("CreateProcess failed");
            return -1;
        }

        IntPtr hJob = CreateJobForProcess(pi.hProcess);

        try
        {
            if (preWaitAction != null)
            {
                preWaitAction(hJob, pi);
            }

            uint r = Helper.WaitForSingleObject(pi.hProcess, uint.MaxValue); // INFINITE
            if (r != 0) // WAIT_OBJECT_0
            {
                Console.Error.WriteLine("Wait failed; exit code {0}", r);
                return -1;
            }

            uint exitCode;
            if (!Helper.GetExitCodeProcess(pi.hProcess, out exitCode))
            {
                Console.Error.WriteLine("GetExitCodeProcess failed");
                return -1;
            }

            return (int)exitCode;
        }
        finally
        {
            if (pi.hProcess != IntPtr.Zero) Helper.CloseHandle(pi.hProcess);
            if (pi.hThread != IntPtr.Zero) Helper.CloseHandle(pi.hThread);
            if (hJob != IntPtr.Zero) Helper.CloseHandle(hJob);
        }
    }

    private static IntPtr CreateJobForProcess(IntPtr hProcess)
    {
        IntPtr hJob = Executor.CreateJobObject(IntPtr.Zero, null);
        if (hJob != IntPtr.Zero)
        {
            Executor.JOBOBJECT_EXTENDED_LIMIT_INFORMATION extInfo = new Executor.JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
            extInfo.BasicLimitInformation = new Executor.JOBOBJECT_BASIC_LIMIT_INFORMATION();
            extInfo.BasicLimitInformation.LimitFlags = 0x2000; // JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE

            int length = Marshal.SizeOf(extInfo);
            IntPtr pExtInfo = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.StructureToPtr(extInfo, pExtInfo, false);
                Executor.SetInformationJobObject(hJob, 9, pExtInfo, (uint)length);
                Executor.AssignProcessToJobObject(hJob, hProcess);
            }
            finally
            {
                Marshal.FreeHGlobal(pExtInfo);
            }
        }
        return hJob;
    }

    public static int StdExecuteCommand(string commandLine){
        return ExecuteCore(
            commandLine,
            0x00000200, // CREATE_NEW_PROCESS_GROUP
            delegate(Helper.STARTUPINFO si) {
                si.dwFlags = 0x00000100; // STARTF_USESTDHANDLES
                si.hStdInput = Helper.GetStdHandle(Helper.STD_INPUT_HANDLE);
                si.hStdOutput = Helper.GetStdHandle(Helper.STD_OUTPUT_HANDLE);
                si.hStdError = Helper.GetStdHandle(Helper.STD_ERROR_HANDLE);
            },
            delegate(IntPtr hJob, Helper.PROCESS_INFORMATION pi) {
                Helper.SetConsoleCtrlHandler(new Helper.ConsoleCtrlDelegate(new Handler((uint)pi.dwProcessId).HandlerRoutine), true);
            }
        );
    }
}
