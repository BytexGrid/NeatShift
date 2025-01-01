using System;

namespace NeatShift.Models
{
    public class NeatSavesOperation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Description { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public string SourcePath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public bool IsSymbolicLinkCreated { get; set; }
        public bool IsRestorePointCreated { get; set; }
    }
} 