using System.IO;

namespace NeatShift.Models
{
    public class FileSystemItem
    {
        public string Path { get; set; }
        public bool IsDirectory { get; set; }
        public string Name => System.IO.Path.GetFileName(Path);

        public FileSystemItem(string path)
        {
            Path = path;
            IsDirectory = Directory.Exists(path);
        }

        public FileSystemItem()
        {
            Path = string.Empty;
            IsDirectory = false;
        }
    }
} 