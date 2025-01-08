using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading.Tasks;
using ModernWpf.Controls;
using NeatShift.Views;

namespace NeatShift.Services
{
    public static class AdminManager
    {
        private static bool _isAdminGranted = false;

        public static bool IsAdminGranted
        {
            get
            {
                if (_isAdminGranted) return true;
                
                // Double check if we're actually running as admin
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                _isAdminGranted = principal.IsInRole(WindowsBuiltInRole.Administrator);
                
                return _isAdminGranted;
            }
        }

        public static async Task<bool> EnsureAdmin(string reason, string actions)
        {
            if (IsAdminGranted) return true;

            // Show our custom prompt first
            var dialog = new AdminPromptDialog(reason, actions);
            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
                return false;

            try
            {
                // Try to restart with admin rights
                var processInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = Process.GetCurrentProcess().MainModule?.FileName,
                    Verb = "runas"
                };

                Process.Start(processInfo);
                // Shutdown the current instance
                System.Windows.Application.Current.Shutdown();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static class Messages
        {
            public static (string Reason, string Actions) SymbolicLink => (
                "Moving files with symbolic links requires administrator access to create special file system links.",
                "• Create symbolic links\n• Maintain file access for applications"
            );

            public static (string Reason, string Actions) SystemRestore => (
                "Creating system restore points requires administrator access to modify system protection settings.",
                "• Create system restore points\n• Manage system protection"
            );

            public static (string Reason, string Actions) ViewLinks => (
                "Viewing symbolic links requires administrator access to read special file system attributes.",
                "• Read symbolic link information\n• View file system attributes"
            );

            public static (string Reason, string Actions) Backup => (
                "Managing backups requires administrator access to create system restore points and manage file system operations.",
                "• Create and manage system restore points\n• Access protected file locations for NeatSaves\n• Manage system protection settings"
            );
        }
    }
} 