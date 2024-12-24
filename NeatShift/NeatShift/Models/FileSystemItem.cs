using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NeatShift.Models
{
    public partial class FileSystemItem : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _path = string.Empty;

        [ObservableProperty]
        private bool _isDirectory;

        public FileSystemItem()
        {
        }

        public FileSystemItem(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
            IsDirectory = Directory.Exists(path);
        }
    }
} 