using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NeatShift.Services
{
    [SupportedOSPlatform("windows")]
    public static class IOHelper
    {
        [SupportedOSPlatform("windows")]
        public static bool HasWritePermission(string path)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(path);
                var security = directoryInfo.GetAccessControl();
                var rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

                var currentUser = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(currentUser);

                foreach (FileSystemAccessRule rule in rules)
                {
                    if (currentUser.User?.Equals(rule.IdentityReference) == true ||
                        principal.IsInRole((SecurityIdentifier)rule.IdentityReference))
                    {
                        if ((rule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write)
                        {
                            if (rule.AccessControlType == AccessControlType.Allow)
                                return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        [SupportedOSPlatform("windows")]
        public static (bool success, string errorMessage) CreateSymbolicLink(string linkPath, string targetPath, bool isDirectory, bool hideLink = true)
        {
            try
            {
                // Check if target exists
                if (!File.Exists(targetPath) && !Directory.Exists(targetPath))
                {
                    return (false, "Target file or directory does not exist");
                }

                // Check if link already exists
                if (File.Exists(linkPath) || Directory.Exists(linkPath))
                {
                    return (false, "A file or directory already exists at the link location");
                }

                // Check if we have write permission to the link location
                string linkDirectory = Path.GetDirectoryName(linkPath) ?? string.Empty;
                if (!string.IsNullOrEmpty(linkDirectory) && !Directory.Exists(linkDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(linkDirectory);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return (false, "No permission to create directory at the link location");
                    }
                }

                bool success = NativeMethods.CreateSymbolicLink(
                    linkPath,
                    targetPath,
                    isDirectory ? NativeMethods.SymbolicLink.Directory : NativeMethods.SymbolicLink.File);

                if (!success)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    string errorMessage = errorCode switch
                    {
                        5 => "Access denied. Administrator privileges are required to create symbolic links",
                        183 => "A file or directory already exists at the link location",
                        1314 => "The client does not have the required privileges",
                        _ => $"Failed to create symbolic link (Error code: {errorCode})"
                    };
                    return (false, errorMessage);
                }

                if (success && hideLink)
                {
                    if (!HideSymbolicLink(linkPath))
                    {
                        return (false, "Symbolic link created but failed to hide it");
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

        public static bool HideSymbolicLink(string path)
        {
            try
            {
                var attributes = File.GetAttributes(path);
                // Add Hidden and System attributes while preserving existing attributes
                attributes |= FileAttributes.Hidden | FileAttributes.System;
                File.SetAttributes(path, attributes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ShowSymbolicLink(string path)
        {
            try
            {
                var attributes = File.GetAttributes(path);
                // Remove Hidden and System attributes while preserving others
                attributes &= ~(FileAttributes.Hidden | FileAttributes.System);
                File.SetAttributes(path, attributes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsSymbolicLink(string path)
        {
            try
            {
                var attributes = File.GetAttributes(path);
                return attributes.HasFlag(FileAttributes.ReparsePoint);
            }
            catch
            {
                return false;
            }
        }

        public static string GetSymbolicLinkTarget(string path)
        {
            try
            {
                if (!IsSymbolicLink(path))
                    return string.Empty;

                var targetPath = Path.GetFullPath(path);
                if (Directory.Exists(targetPath))
                {
                    var info = new DirectoryInfo(targetPath);
                    return Path.GetFullPath(info.LinkTarget ?? string.Empty);
                }
                else if (File.Exists(targetPath))
                {
                    var info = new FileInfo(targetPath);
                    return Path.GetFullPath(info.LinkTarget ?? string.Empty);
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static IEnumerable<(string Path, string Target, bool IsHidden)> GetSymbolicLinks(string directoryPath)
        {
            try
            {
                var directory = new DirectoryInfo(directoryPath);
                var results = new List<(string, string, bool)>();

                // Get all files and directories, including hidden ones
                var items = directory.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly);

                foreach (var item in items)
                {
                    if (item.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        bool isHidden = item.Attributes.HasFlag(FileAttributes.Hidden);
                        string target = string.Empty;

                        if (item is DirectoryInfo dirInfo)
                        {
                            target = dirInfo.LinkTarget ?? string.Empty;
                        }
                        else if (item is FileInfo fileInfo)
                        {
                            target = fileInfo.LinkTarget ?? string.Empty;
                        }

                        results.Add((item.FullName, target, isHidden));
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting symbolic links: {ex.Message}");
                return Enumerable.Empty<(string, string, bool)>();
            }
        }

        public static IEnumerable<(string Path, string Target, bool IsHidden)> GetAllSymbolicLinks(string directoryPath)
        {
            try
            {
                var directory = new DirectoryInfo(directoryPath);
                var results = new List<(string, string, bool)>();

                // Get all files and directories recursively, including hidden ones
                var items = directory.GetFileSystemInfos("*", SearchOption.AllDirectories);

                foreach (var item in items)
                {
                    if (item.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        bool isHidden = item.Attributes.HasFlag(FileAttributes.Hidden);
                        string target = string.Empty;

                        if (item is DirectoryInfo dirInfo)
                        {
                            target = dirInfo.LinkTarget ?? string.Empty;
                        }
                        else if (item is FileInfo fileInfo)
                        {
                            target = fileInfo.LinkTarget ?? string.Empty;
                        }

                        results.Add((item.FullName, target, isHidden));
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting symbolic links: {ex.Message}");
                return Enumerable.Empty<(string, string, bool)>();
            }
        }

        public static bool ToggleSymbolicLinkVisibility(string path)
        {
            try
            {
                var attributes = File.GetAttributes(path);
                if (attributes.HasFlag(FileAttributes.Hidden))
                {
                    // Show the link
                    attributes &= ~(FileAttributes.Hidden | FileAttributes.System);
                }
                else
                {
                    // Hide the link
                    attributes |= FileAttributes.Hidden | FileAttributes.System;
                }
                File.SetAttributes(path, attributes);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling symbolic link visibility: {ex.Message}");
                return false;
            }
        }

        [SupportedOSPlatform("windows")]
        public static bool DeleteSymbolicLink(string path)
        {
            try
            {
                if (!IsSymbolicLink(path))
                    return false;

                // Remove read-only attribute if present
                var attributes = File.GetAttributes(path);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
                }

                // For directories, use Directory.Delete
                if (Directory.Exists(path))
                {
                    Directory.Delete(path);
                    return true;
                }

                // For files, use File.Delete
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting symbolic link: {ex.Message}");
                return false;
            }
        }

        private static void RemoveDirectoryWithRetry(string path, int retries = 3)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    Directory.Delete(path);
                    return;
                }
                catch (IOException) when (i < retries - 1)
                {
                    System.Threading.Thread.Sleep(100); // Wait briefly before retry
                }
            }
            throw new IOException($"Failed to delete directory after {retries} attempts");
        }

        private static void RemoveFileWithRetry(string path, int retries = 3)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    File.Delete(path);
                    return;
                }
                catch (IOException) when (i < retries - 1)
                {
                    System.Threading.Thread.Sleep(100); // Wait briefly before retry
                }
            }
            throw new IOException($"Failed to delete file after {retries} attempts");
        }
    }
} 