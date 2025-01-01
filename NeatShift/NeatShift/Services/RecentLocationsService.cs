using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace NeatShift.Services
{
    public interface IRecentLocationsService
    {
        List<string> LoadRecentLocations();
        void SaveRecentLocations(IEnumerable<string> locations);
    }

    public class RecentLocationsService : IRecentLocationsService
    {
        private readonly string _filePath;

        public RecentLocationsService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NeatShift"
            );
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _filePath = Path.Combine(appDataPath, "recent_locations.json");
        }

        public List<string> LoadRecentLocations()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    var locations = JsonSerializer.Deserialize<List<string>>(json);
                    return locations ?? new List<string>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recent locations: {ex.Message}");
            }

            return new List<string>();
        }

        public void SaveRecentLocations(IEnumerable<string> locations)
        {
            try
            {
                var json = JsonSerializer.Serialize(locations);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving recent locations: {ex.Message}");
            }
        }
    }
} 