using System;
using System.Runtime.InteropServices;

namespace NeatShift.Services
{
    internal static class NativeMethods
    {
        public const int BEGIN_SYSTEM_CHANGE = 100;
        public const int MODIFY_SETTINGS = 12;

        [StructLayout(LayoutKind.Sequential)]
        public struct RESTORE_POINT
        {
            public int dwEventType;
            public int dwRestorePtType;
            public Int64 llSequenceNumber;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szDescription;
        }

        [DllImport("srrestorept.dll", EntryPoint = "SRSetRestorePointW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int SRSetRestorePoint(RESTORE_POINT? restorePoint, out int status);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        public enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }
    }
} 