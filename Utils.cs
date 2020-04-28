using System.Collections.Generic;
namespace Utils
{
    using System.Runtime.InteropServices;
    using System;

    public class Vec3
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;
    }


    public class Offsets
    {
        public static int HEALTH_BASE_OFFSET = 0x0095B184; //offset from module bfvietnam.exe
        public static int V_VECTOR_BASE_OFFSET = 0x0097D01C; //offset from module bfvietnam.exe

        public static int[] HEALTH_OFFSETS = new int[] { 0x170, 0xDC, 0x0, 0x10, 0x38 };
        public static int[] X_VECTOR_OFFSETS = new int[] { 0x54, 0xA4, 0xC4 };

        //player y seems to be 0.60000038 below camera 6 coords

    }

    public class Constants
    {
        //===============================================
        //                  CONST
        //===============================================
        public static uint PROCESS_VM_READ = 0x0010;
        public static uint PROCESS_VM_WRITE = 0x0020;
        public static uint PROCESS_VM_OPERATION = 0x0008;
        public static uint PAGE_READWRITE = 0x0004;
        public static String[] PROCESSES_TO_IGNORE = { "chrome", "svcHost", "svchost", "Code", "steamwebhelper" };
    }

    public class Pinvoke
    {
        //===============================================
        //                  PINVOKE
        //===============================================

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, long lpBaseAddress, [In, Out] byte[] lpBuffer, ulong dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hProcess);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);
        // Utils.Pinvoke.MessageBox(IntPtr.Zero, "Command-line message box", "Attention!", 0);


        //===============================================
        //                  FLAGS
        //===============================================

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }
    }
}