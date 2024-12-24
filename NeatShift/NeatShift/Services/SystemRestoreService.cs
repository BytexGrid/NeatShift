using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;

namespace NeatShift.Services
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class SystemRestoreService : ISystemRestoreService
    {
        private bool _isSystemRestoreAvailable;

        public SystemRestoreService()
        {
            try
            {
                Debug.WriteLine("Initializing SystemRestoreService...");

                // First check if srrestorept.dll is available
                var dllHandle = LoadLibrary("srrestorept.dll");
                if (dllHandle == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    Debug.WriteLine($"Failed to load srrestorept.dll. Error code: {error} ({new Win32Exception(error).Message})");
                    _isSystemRestoreAvailable = false;
                    return;
                }
                Debug.WriteLine("Successfully loaded srrestorept.dll");
                FreeLibrary(dllHandle);

                // Try to create a test restore point to check if the functionality is available
                var testPoint = new RESTOREPOINTINFO
                {
                    dwEventType = (int)RestoreType.ApplicationInstall,
                    dwRestorePtType = (int)RestoreType.ApplicationInstall,
                    llSequenceNumber = 0,
                    szDescription = "NeatShift Test Restore Point"
                };

                Debug.WriteLine("Attempting to create test restore point...");
                _isSystemRestoreAvailable = SRSetRestorePoint(ref testPoint, out STATEMGRSTATUS status);
                int lastError = Marshal.GetLastWin32Error();
                
                Debug.WriteLine($"System restore test results:");
                Debug.WriteLine($"- Available: {_isSystemRestoreAvailable}");
                Debug.WriteLine($"- Status code: {status.nStatus}");
                Debug.WriteLine($"- Status description: {GetStatusDescription(status.nStatus)}");
                Debug.WriteLine($"- Last error: {lastError} ({new Win32Exception(lastError).Message})");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize system restore: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex is DllNotFoundException dllEx)
                {
                    Debug.WriteLine($"DLL not found: {dllEx.Message}");
                }
                _isSystemRestoreAvailable = false;
            }
        }

        public async Task<bool> CreateRestorePoint(string description)
        {
            Debug.WriteLine($"CreateRestorePoint called with description: {description}");
            
            if (!_isSystemRestoreAvailable)
            {
                Debug.WriteLine("System restore is not available, skipping restore point creation");
                return false;
            }

            return await Task.Run(() =>
            {
                try
                {
                    Debug.WriteLine($"Creating restore point: {description}");
                    var restorePoint = new RESTOREPOINTINFO
                    {
                        dwEventType = (int)RestoreType.ApplicationInstall,
                        dwRestorePtType = (int)RestoreType.ApplicationInstall,
                        llSequenceNumber = 0,
                        szDescription = description
                    };

                    bool success = SRSetRestorePoint(ref restorePoint, out STATEMGRSTATUS status);
                    int lastError = Marshal.GetLastWin32Error();
                    
                    Debug.WriteLine($"Restore point creation results:");
                    Debug.WriteLine($"- Success: {success}");
                    Debug.WriteLine($"- Status code: {status.nStatus}");
                    Debug.WriteLine($"- Status description: {GetStatusDescription(status.nStatus)}");
                    Debug.WriteLine($"- Last error: {lastError} ({new Win32Exception(lastError).Message})");
                    Debug.WriteLine($"- Sequence number: {status.llSequenceNumber}");
                    
                    return success;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error creating restore point: {ex.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    return false;
                }
            });
        }

        private string GetStatusDescription(int statusCode)
        {
            switch (statusCode)
            {
                case 0: return "The operation was successful";
                case 1: return "Restore point creation in progress";
                case 2: return "The system restore was disabled";
                case 3: return "The system restore service was not available";
                case 4: return "The operation failed";
                case 5: return "Another operation was already in progress";
                default: return $"Unknown status code: {statusCode}";
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("srrestorept.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SRSetRestorePoint(ref RESTOREPOINTINFO restorePointInfo, out STATEMGRSTATUS status);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct RESTOREPOINTINFO
        {
            public int dwEventType;
            public int dwRestorePtType;
            public long llSequenceNumber;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STATEMGRSTATUS
        {
            public int nStatus;
            public long llSequenceNumber;
        }

        private enum RestoreType
        {
            ApplicationInstall = 0,
            ApplicationUninstall = 1,
            ModifySettings = 12,
            CancelledOperation = 13
        }
    }
} 