using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NeatShift.Services
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class FileOperationService : IFileOperationService
    {
        private readonly ISystemRestoreService _systemRestoreService;
        private readonly ISettingsService _settingsService;

        public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

        public FileOperationService(ISystemRestoreService systemRestoreService, ISettingsService settingsService)
        {
            _systemRestoreService = systemRestoreService;
            _settingsService = settingsService;
        }

        public async Task<bool> MoveWithSymbolicLink(string sourcePath, string destinationPath)
        {
            try
            {
                OnProgressChanged(0, $"Preparing to move {Path.GetFileName(sourcePath)}...");

                // Check if the destination already exists
                if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
                {
                    OnProgressChanged(0, "Error: A file or folder with the same name already exists in the destination.");
                    return false;
                }

                if (_settingsService.GetCreateRestorePoint())
                {
                    OnProgressChanged(10, "Creating system restore point...");
                    bool restorePointCreated = await _systemRestoreService.CreateRestorePoint("Before moving files");
                    if (!restorePointCreated)
                    {
                        Debug.WriteLine("Failed to create restore point");
                        OnProgressChanged(10, "Warning: Could not create system restore point. This may require administrator privileges.");
                        // Continue with the operation even if restore point creation fails
                    }
                    else
                    {
                        OnProgressChanged(20, "System restore point created successfully");
                    }
                }

                OnProgressChanged(30, "Moving files...");
                string? destinationDir = Path.GetDirectoryName(destinationPath);
                if (string.IsNullOrEmpty(destinationDir))
                {
                    OnProgressChanged(0, "Error: Invalid destination path");
                    return false;
                }

                Directory.CreateDirectory(destinationDir);

                try
                {
                    if (Directory.Exists(sourcePath))
                    {
                        Directory.Move(sourcePath, destinationPath);
                    }
                    else
                    {
                        File.Move(sourcePath, destinationPath);
                    }
                }
                catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 183) // File already exists
                {
                    OnProgressChanged(0, "Error: A file or folder with the same name already exists in the destination.");
                    return false;
                }

                OnProgressChanged(60, "Creating symbolic link...");
                bool hideLink = _settingsService.GetHideSymbolicLinks();
                bool success = IOHelper.CreateSymbolicLink(sourcePath, destinationPath, Directory.Exists(destinationPath), hideLink);

                if (success)
                {
                    OnProgressChanged(100, "Operation completed successfully");
                    return true;
                }

                OnProgressChanged(0, "Failed to create symbolic link. This may require administrator privileges.");
                return false;
            }
            catch (Exception ex)
            {
                OnProgressChanged(0, $"Error: {ex.Message}");
                return false;
            }
        }

        protected virtual void OnProgressChanged(int progressPercentage, string message)
        {
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs 
            { 
                ProgressPercentage = progressPercentage, 
                Message = message 
            });
        }
    }
} 