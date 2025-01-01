using System;
using System.Threading.Tasks;

namespace NeatShift.Services
{
    public interface IFileOperationService
    {
        event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        Task<bool> MoveWithSymbolicLink(string sourcePath, string destinationPath);
    }

    public class ProgressChangedEventArgs : EventArgs
    {
        public int ProgressPercentage { get; set; }
        public string Message { get; set; } = string.Empty;
    }
} 