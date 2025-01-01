using System;
using System.IO;

namespace NeatShift.Models
{
    public class NeatSavesSettings
    {
        public bool UseNeatSaves { get; set; } = false;
        public int MaxOperationsToKeep { get; set; } = 50;
        public string SaveLocation { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NeatShift",
            "NeatSaves"
        );
    }
} 