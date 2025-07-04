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
            string errorMessage;
            try
            {
                OnProgressChanged(0, $"Preparing to move {Path.GetFileName(sourcePath)}...");

                // Check if the destination already exists
                if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
                {
                    errorMessage = "Error: A file or folder with the same name already exists in the destination.";
                    OnProgressChanged(0, errorMessage);
                    AppLogger.Log(errorMessage);
                    return false;
                }

                // Check if source exists
                if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
                {
                    errorMessage = "Error: The source file or folder does not exist.";
                    OnProgressChanged(0, errorMessage);
                    AppLogger.Log(errorMessage);
                    return false;
                }

                // Check if we have write permission to the destination
                string destDirectory = Path.GetDirectoryName(destinationPath) ?? string.Empty;
                if (!string.IsNullOrEmpty(destDirectory))
                {
                    try
                    {
                        if (!Directory.Exists(destDirectory))
                        {
                            Directory.CreateDirectory(destDirectory);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        errorMessage = "Error: No permission to create directory at the destination location.";
                        OnProgressChanged(0, errorMessage);
                        AppLogger.Log(errorMessage);
                        return false;
                    }
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

                OnProgressChanged(20, "Moving file/folder...");
                try
                {
                    // Use Task.Run for potentially long-running file operations
                    await Task.Run(() =>
                    {
                        bool sameRoot = string.Equals(Path.GetPathRoot(sourcePath), Path.GetPathRoot(destinationPath), StringComparison.OrdinalIgnoreCase);
                        if (Directory.Exists(sourcePath))
                        {
                            if (sameRoot)
                            {
                                Directory.Move(sourcePath, destinationPath);
                            }
                            else
                            {
                                IOHelper.CopyDirectoryRecursive(sourcePath, destinationPath);
                                Directory.Delete(sourcePath, true);
                            }
                        }
                        else
                        {
                            if (sameRoot)
                            {
                                File.Move(sourcePath, destinationPath);
                            }
                            else
                            {
                                File.Copy(sourcePath, destinationPath);
                                File.Delete(sourcePath);
                            }
                        }
                    });
                }
                catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 183) // File already exists
                {
                    errorMessage = "Error: A file or folder with the same name already exists in the destination.";
                    OnProgressChanged(0, errorMessage);
                    AppLogger.Log(errorMessage);
                    return false;
                }
                catch (UnauthorizedAccessException)
                {
                    errorMessage = "Error: No permission to move the file/folder. Please check your permissions.";
                    OnProgressChanged(0, errorMessage);
                    AppLogger.Log(errorMessage);
                    return false;
                }
                catch (Exception ex)
                {
                    OnProgressChanged(0, $"Error moving file/folder: {ex.Message}");
                    AppLogger.Log(ex);
                    return false;
                }

                OnProgressChanged(60, "Creating symbolic link...");
                bool hideLink = _settingsService.GetHideSymbolicLinks();
                var (success, linkErrorMessage) = await Task.Run(() => 
                    IOHelper.CreateSymbolicLink(sourcePath, destinationPath, Directory.Exists(destinationPath), hideLink));

                if (success)
                {
                    OnProgressChanged(100, "Operation completed successfully");
                    return true;
                }

                errorMessage = linkErrorMessage;

                // If symbolic link creation failed, try to move the file back
                try
                {
                    await Task.Run(() =>
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            Directory.Move(destinationPath, sourcePath);
                        }
                        else
                        {
                            File.Move(destinationPath, sourcePath);
                        }
                    });
                }
                catch (Exception ex)
                {
                    OnProgressChanged(0, $"Error: {errorMessage}\nAdditionally, failed to restore the original file location: {ex.Message}");
                    AppLogger.Log(ex);
                    AppLogger.Log($"Symbolic link creation failed with message: {errorMessage} and original file could not be restored.");
                    return false;
                }

                OnProgressChanged(0, $"Error: {errorMessage}");
                AppLogger.Log($"Symbolic link creation failed with message: {errorMessage}");
                return false;
            }
            catch (Exception ex)
            {
                OnProgressChanged(0, $"Unexpected error: {ex.Message}");
                AppLogger.Log(ex);
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