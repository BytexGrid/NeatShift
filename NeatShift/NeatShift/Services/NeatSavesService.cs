using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using NeatShift.Models;

namespace NeatShift.Services
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class NeatSavesService : INeatSavesService
    {
        private readonly string _operationsFile;
        private readonly NeatSavesSettings _settings;
        private List<NeatSavesOperation> _operations;
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;

        public NeatSavesService(NeatSavesSettings settings)
        {
            _settings = settings;
            _operations = new List<NeatSavesOperation>();
            _operationsFile = Path.Combine(_settings.SaveLocation, "operations.json");
        }

        public async Task<bool> Initialize()
        {
            try
            {
                if (!Directory.Exists(_settings.SaveLocation))
                {
                    Directory.CreateDirectory(_settings.SaveLocation);
                }

                if (File.Exists(_operationsFile))
                {
                    string json = await File.ReadAllTextAsync(_operationsFile);
                    _operations = JsonSerializer.Deserialize<List<NeatSavesOperation>>(json) ?? new List<NeatSavesOperation>();
                }

                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing NeatSaves: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateNeatSave(string sourcePath, string destinationPath, string description)
        {
            if (!_isInitialized) return false;

            try
            {
                var operation = new NeatSavesOperation
                {
                    Description = description,
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath,
                    CreationTime = DateTime.Now
                };

                // Add to operations list
                _operations.Add(operation);
                
                // Maintain max operations limit
                while (_operations.Count > _settings.MaxOperationsToKeep)
                {
                    var oldestOp = _operations.OrderBy(o => o.CreationTime).First();
                    await DeleteNeatSave(oldestOp.Id);
                }

                // Save operations list
                await SaveOperations();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating NeatSave: {ex.Message}");
                return false;
            }
        }

        public List<NeatSavesOperation> GetNeatSaves()
        {
            return _operations.OrderByDescending(o => o.CreationTime).ToList();
        }

        public async Task<(bool success, string operationId)> RestoreNeatSave(string operationId)
        {
            if (!_isInitialized) return (false, string.Empty);

            try
            {
                var operation = _operations.FirstOrDefault(o => o.Id == operationId);
                if (operation == null) return (false, string.Empty);

                // Split the source paths (they were joined with semicolons during save)
                var sourcePaths = operation.SourcePath.Split(';', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var sourcePath in sourcePaths)
                {
                    // Get the corresponding destination path
                    string fileName = Path.GetFileName(sourcePath);
                    string destinationPath = Path.Combine(operation.DestinationPath, fileName);

                    // Verify destination exists
                    if (!File.Exists(destinationPath) && !Directory.Exists(destinationPath))
                    {
                        throw new FileNotFoundException($"The moved file no longer exists at destination: {destinationPath}");
                    }

                    // Delete symbolic link at source if it exists
                    if (File.Exists(sourcePath))
                    {
                        var attributes = File.GetAttributes(sourcePath);
                        if ((attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                        {
                            File.Delete(sourcePath);
                        }
                    }
                    else if (Directory.Exists(sourcePath))
                    {
                        var attributes = File.GetAttributes(sourcePath);
                        if ((attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                        {
                            Directory.Delete(sourcePath);
                        }
                    }

                    // Move file back to original location
                    if (File.Exists(destinationPath))
                    {
                        File.Move(destinationPath, sourcePath, true);
                    }
                    else if (Directory.Exists(destinationPath))
                    {
                        if (Directory.Exists(sourcePath))
                        {
                            Directory.Delete(sourcePath, true); // Remove existing directory if any
                        }
                        Directory.Move(destinationPath, sourcePath);
                    }
                }

                return (true, operationId); // Return both success and operationId
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error restoring NeatSave: {ex.Message}");
                return (false, string.Empty);
            }
        }

        public async Task<bool> DeleteNeatSave(string operationId)
        {
            if (!_isInitialized) return false;

            try
            {
                var operation = _operations.FirstOrDefault(o => o.Id == operationId);
                if (operation == null) return false;

                _operations.Remove(operation);
                await SaveOperations();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting NeatSave: {ex.Message}");
                return false;
            }
        }

        private async Task SaveOperations()
        {
            string json = JsonSerializer.Serialize(_operations);
            await File.WriteAllTextAsync(_operationsFile, json);
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Copy all files
            foreach (string filePath in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(filePath);
                string destFile = Path.Combine(destinationDir, fileName);
                File.Copy(filePath, destFile);
            }

            // Copy all subdirectories
            foreach (string dirPath in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(dirPath);
                string destDir = Path.Combine(destinationDir, dirName);
                CopyDirectory(dirPath, destDir);
            }
        }
    }
} 