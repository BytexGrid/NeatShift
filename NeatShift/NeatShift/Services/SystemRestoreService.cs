//    NeatShift - Easily move files while keeping them accessible
//    Copyright (C) 2024 BytexGrid
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using NeatShift.Models;
using System.Linq;
using System.Runtime.InteropServices;

namespace NeatShift.Services
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class SystemRestoreService : ISystemRestoreService
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        private bool? _isSystemRestoreAvailable;

        public SystemRestoreService()
        {
            Debug.WriteLine("Initializing SystemRestoreService...");
        }

        private async Task<bool> IsSystemRestoreAvailable()
        {
            if (_isSystemRestoreAvailable.HasValue)
                return _isSystemRestoreAvailable.Value;

            try
            {
                // Just try to launch the System Protection dialog to check if it's available
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "SystemPropertiesProtection.exe",
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                });

                if (process != null)
                {
                    await Task.Delay(500); // Give it a moment to start
                    process.Kill(); // Close it immediately
                    _isSystemRestoreAvailable = true;
                }
                else
                {
                    _isSystemRestoreAvailable = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking system restore availability: {ex.Message}");
                _isSystemRestoreAvailable = false;
            }

            return _isSystemRestoreAvailable.Value;
        }

        public async Task<List<RestorePoint>> GetRestorePoints()
        {
            var restorePoints = new List<RestorePoint>();

            try
            {
                // Use PowerShell to get restore points
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = @"-NoProfile -Command ""& {
                            Get-ComputerRestorePoint | ForEach-Object {
                                $seq = $_.SequenceNumber
                                $timeStr = $_.CreationTime
                                $desc = $_.Description
                                Write-Output ($seq.ToString() + '|' + $timeStr + '|' + $desc)
                            }
                        }""",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                Debug.WriteLine($"PowerShell output: {output}");
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine($"PowerShell error: {error}");
                }

                // Parse the output line by line
                var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length == 3)
                    {
                        if (int.TryParse(parts[0], out int sequenceNumber))
                        {
                            // Parse the special format: yyyyMMddHHmmss.ffffff-000
                            var timeStr = parts[1].Trim();
                            if (timeStr.Length >= 14) // At least yyyyMMddHHmmss
                            {
                                try
                                {
                                    var creationTime = ParseRestorePointTime(timeStr);
                                    
                                    restorePoints.Add(new RestorePoint
                                    {
                                        Id = sequenceNumber.ToString(),
                                        Description = parts[2].Trim(),
                                        CreationTime = creationTime
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Error parsing time {timeStr}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting restore points: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            // Sort by creation time (newest first)
            return restorePoints.OrderByDescending(p => p.CreationTime).ToList();
        }

        public async Task<bool> CreateRestorePoint(string description)
        {
            if (!await IsSystemRestoreAvailable())
            {
                Debug.WriteLine("System restore is not available");
                return false;
            }

            try
            {
                // Get current restore points to compare after creation
                var beforePoints = await GetRestorePoints();
                var beforeCount = beforePoints.Count;
                var lastSequence = beforePoints.Any() ? int.Parse(beforePoints.First().Id) : 0;

                Debug.WriteLine($"Before creation - Points: {beforeCount}, Last sequence: {lastSequence}");

                // Create restore point using PowerShell
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $@"-Command ""& {{
                            Enable-ComputerRestore -Drive 'C:\';
                            Checkpoint-Computer -Description '{description}' -RestorePointType 'MODIFY_SETTINGS'
                        }}""",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        Verb = "runas",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                Debug.WriteLine($"Create restore point results:");
                Debug.WriteLine($"- Output: {output}");
                Debug.WriteLine($"- Error: {error}");
                Debug.WriteLine($"- Exit code: {process.ExitCode}");

                // Check for 3-minute warning in both output and error
                if (output.Contains("cannot be created because one has already been created within the past 3 minutes") ||
                    error.Contains("cannot be created because one has already been created within the past 3 minutes") ||
                    output.Contains("WARNING: A new system restore point cannot be created") ||
                    error.Contains("WARNING: A new system restore point cannot be created"))
                {
                    Debug.WriteLine("3-minute cooldown warning detected");
                    return false;
                }

                // Wait a moment for the restore point to be registered
                await Task.Delay(1000);

                // Verify the restore point was created
                var afterPoints = await GetRestorePoints();
                var afterCount = afterPoints.Count;
                var newSequence = afterPoints.Any() ? int.Parse(afterPoints.First().Id) : 0;

                Debug.WriteLine($"After creation - Points: {afterCount}, New sequence: {newSequence}");

                bool success = process.ExitCode == 0 && 
                             afterCount > beforeCount && 
                             newSequence > lastSequence;

                Debug.WriteLine($"Creation verification:");
                Debug.WriteLine($"- Exit code success: {process.ExitCode == 0}");
                Debug.WriteLine($"- Count increased: {afterCount > beforeCount}");
                Debug.WriteLine($"- New sequence: {newSequence > lastSequence}");
                Debug.WriteLine($"- Overall success: {success}");
                    
                    return success;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error creating restore point: {ex.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    return false;
                }
        }

        private DateTime ParseRestorePointTime(string timeStr)
        {
            try
            {
                // Format: yyyyMMddHHmmss.ffffff-000
                var year = int.Parse(timeStr.Substring(0, 4));
                var month = int.Parse(timeStr.Substring(4, 2));
                var day = int.Parse(timeStr.Substring(6, 2));
                var hour = int.Parse(timeStr.Substring(8, 2));
                var minute = int.Parse(timeStr.Substring(10, 2));
                var second = int.Parse(timeStr.Substring(12, 2));

                // Convert to local time since PowerShell returns UTC
                var utcTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
                return utcTime.ToLocalTime();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing time {timeStr}: {ex.Message}");
                return DateTime.Now; // Fallback to current time
            }
        }

        public async Task<bool> DeleteRestorePoint(string sequenceNumber)
        {
            try
            {
                // First try using srdeletepoint.exe (Windows built-in tool)
                using (var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -Command \"& {{ srdeletepoint.exe /n {sequenceNumber} }}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        Verb = "runas",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    Debug.WriteLine($"srdeletepoint results:");
                    Debug.WriteLine($"- Output: {output}");
                    Debug.WriteLine($"- Error: {error}");
                    Debug.WriteLine($"- Exit code: {process.ExitCode}");

                    // Check if the point still exists
                    var points = await GetRestorePoints();
                    bool pointStillExists = points.Any(p => p.Id == sequenceNumber);
                    
                    if (!pointStillExists)
                    {
                        Debug.WriteLine("Restore point deleted successfully via srdeletepoint");
                        return true;
                    }
                }

                // If srdeletepoint failed, try using vssadmin to delete the shadow copy
                Debug.WriteLine("srdeletepoint failed, trying vssadmin...");
                
                // Get the restore point's creation time
                var allPoints = await GetRestorePoints();
                var targetPoint = allPoints.FirstOrDefault(p => p.Id == sequenceNumber);
                if (targetPoint == null)
                {
                    Debug.WriteLine("Could not find target restore point");
                    return false;
                }

                // Get shadow copies and find the matching one
                using (var listProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c vssadmin list shadows",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                })
                {
                    listProcess.Start();
                    string listOutput = await listProcess.StandardOutput.ReadToEndAsync();
                    await listProcess.WaitForExitAsync();

                    Debug.WriteLine($"VSS list output: {listOutput}");

                    // Find the shadow copy ID for our restore point's time
                    string? shadowId = null;
                    DateTime? shadowTime = null;
                    var lines = listOutput.Split('\n');
                    
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i].Trim();
                        if (line.Contains("creation time:"))
                        {
                            var timeStr = line.Substring(line.IndexOf(':') + 1).Trim();
                            if (DateTime.TryParse(timeStr, out DateTime time))
                            {
                                // Look ahead for Shadow Copy ID
                                for (int j = i + 1; j < Math.Min(i + 5, lines.Length); j++)
                                {
                                    var idLine = lines[j].Trim();
                                    if (idLine.StartsWith("Shadow Copy ID:"))
                                    {
                                        var id = idLine.Substring(idLine.IndexOf('{'));
                                        // If this time is closer to our target time than the previous match
                                        if (shadowTime == null || 
                                            Math.Abs((time - targetPoint.CreationTime).TotalMinutes) < 
                                            Math.Abs((shadowTime.Value - targetPoint.CreationTime).TotalMinutes))
                                        {
                                            shadowId = id;
                                            shadowTime = time;
                                            Debug.WriteLine($"Found potential match: ID={id}, Time={time}");
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (shadowId != null && shadowTime != null)
                    {
                        Debug.WriteLine($"Using shadow ID: {shadowId} (created at {shadowTime})");
                        Debug.WriteLine($"Target point time: {targetPoint.CreationTime}");
                        Debug.WriteLine($"Time difference: {Math.Abs((shadowTime.Value - targetPoint.CreationTime).TotalMinutes)} minutes");
                        
                        // Try to delete the shadow copy
                        using var deleteProcess = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = $"/c vssadmin delete shadows /Shadow={shadowId} /Quiet",
                                UseShellExecute = true,
                                Verb = "runas",
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Hidden
                            }
                        };

                        deleteProcess.Start();
                        await deleteProcess.WaitForExitAsync();

                        // Final check if the restore point was deleted
                        var finalPoints = await GetRestorePoints();
                        bool stillExists = finalPoints.Any(p => p.Id == sequenceNumber);
                        Debug.WriteLine($"After vssadmin delete - Point still exists: {stillExists}");

                        return !stillExists;
                    }
                    else
                    {
                        Debug.WriteLine("Could not find matching shadow copy");
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting restore point: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
} 