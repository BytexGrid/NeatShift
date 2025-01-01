using System;
using System.IO;
using System.Text.Json;
using NeatShift.Models;

namespace NeatShift.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private Models.Settings _currentSettings;
        private readonly ISystemRestoreService _systemRestoreService;

        public SettingsService(ISystemRestoreService systemRestoreService)
        {
            _systemRestoreService = systemRestoreService ?? throw new ArgumentNullException(nameof(systemRestoreService));
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NeatShift",
                "settings.json");
            
            _currentSettings = LoadSettings();
        }

        public Models.Settings LoadSettings()
        {
            if (!File.Exists(_settingsPath))
            {
                _currentSettings = new Models.Settings();
                SaveSettings(_currentSettings);
                return _currentSettings;
            }

            try
            {
                string json = File.ReadAllText(_settingsPath);
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                };
                var settings = JsonSerializer.Deserialize<Models.Settings>(json, options);
                _currentSettings = settings ?? new Models.Settings();
                return _currentSettings;
            }
            catch
            {
                _currentSettings = new Models.Settings();
                SaveSettings(_currentSettings);
                return _currentSettings;
            }
        }

        public void SaveSettings(Models.Settings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            string directory = Path.GetDirectoryName(_settingsPath) ?? 
                throw new InvalidOperationException("Invalid settings path");

            Directory.CreateDirectory(directory);

            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_settingsPath, json);
            _currentSettings = settings;
        }

        public bool GetCreateRestorePoint()
        {
            return _currentSettings.CreateRestorePoint;
        }

        public void SetCreateRestorePoint(bool value)
        {
            if (_currentSettings.CreateRestorePoint != value)
            {
                _currentSettings.CreateRestorePoint = value;
                SaveSettings(_currentSettings);
            }
        }

        public bool GetHideSymbolicLinks()
        {
            return _currentSettings.HideSymbolicLinks;
        }

        public void SetHideSymbolicLinks(bool value)
        {
            if (_currentSettings.HideSymbolicLinks != value)
            {
                _currentSettings.HideSymbolicLinks = value;
                SaveSettings(_currentSettings);
            }
        }

        // NeatSaves settings implementation
        public bool GetUseNeatSaves()
        {
            return _currentSettings.UseNeatSaves;
        }

        public void SetUseNeatSaves(bool value)
        {
            if (_currentSettings.UseNeatSaves != value)
            {
                _currentSettings.UseNeatSaves = value;
                SaveSettings(_currentSettings);
            }
        }

        public int GetMaxNeatSaves()
        {
            return _currentSettings.MaxNeatSaves;
        }

        public void SetMaxNeatSaves(int value)
        {
            if (_currentSettings.MaxNeatSaves != value)
            {
                _currentSettings.MaxNeatSaves = value;
                SaveSettings(_currentSettings);
            }
        }

        public string GetNeatSavesLocation()
        {
            return _currentSettings.NeatSavesLocation;
        }

        public void SetNeatSavesLocation(string path)
        {
            if (_currentSettings.NeatSavesLocation != path)
            {
                _currentSettings.NeatSavesLocation = path;
                SaveSettings(_currentSettings);
            }
        }
    }
} 