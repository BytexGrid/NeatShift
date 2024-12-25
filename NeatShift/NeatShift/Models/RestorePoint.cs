using System;

namespace NeatShift.Models
{
    public class RestorePoint
    {
        public string Description { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
} 