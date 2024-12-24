namespace NeatShift.Models
{
    public class Settings
    {
        public bool CreateRestorePoint { get; set; } = true;
        public bool HideSymbolicLinks { get; set; } = true;
        public string LastUsedPath { get; set; } = string.Empty;
    }
} 