using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using NeatShift.Services;

namespace NeatShift.ViewModels
{
    public partial class FileBrowserViewModel : ObservableObject
    {
        private readonly Stack<string> _backStack = new();
        private readonly Stack<string> _forwardStack = new();
        private readonly ObservableCollection<string> _recentLocations = new();
        private const int MAX_RECENT_LOCATIONS = 10;
        private readonly IRecentLocationsService _recentLocationsService;
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        [ObservableProperty]
        private ObservableCollection<FileItem> _items = new();

        [ObservableProperty]
        private FileItem? _selectedItem;

        [ObservableProperty]
        private string _sortColumn = "Name";

        [ObservableProperty]
        private bool _sortAscending = true;

        [ObservableProperty]
        private bool _isGridView = false;

        [ObservableProperty]
        private bool _isEditingPath = false;

        [ObservableProperty]
        private bool _isSelectionMode = false;

        [ObservableProperty]
        private ObservableCollection<FileItem> _selectedItems = new();

        private string _originalPath = string.Empty;

        [ObservableProperty]
        private ObservableCollection<PathSegment> _pathSegments = new();

        public bool CanNavigateBack => _backStack.Any();
        public bool CanNavigateForward => _forwardStack.Any();

        public ObservableCollection<string> RecentLocations => _recentLocations;

        public FileBrowserViewModel(IRecentLocationsService recentLocationsService)
        {
            _recentLocationsService = recentLocationsService;
            UpdatePathSegments();
            SelectedItems.CollectionChanged += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"Selected items count: {SelectedItems.Count}");
            };
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            LoadItems(CurrentPath);

            var savedLocations = _recentLocationsService.LoadRecentLocations();
            foreach (var location in savedLocations.Take(MAX_RECENT_LOCATIONS))
            {
                _recentLocations.Add(location);
            }

            _isInitialized = true;
        }

        private void UpdatePathSegments()
        {
            PathSegments.Clear();
            var path = CurrentPath;
            var segments = new List<PathSegment>();

            // Add drive root
            var root = Path.GetPathRoot(path);
            if (!string.IsNullOrEmpty(root))
            {
                segments.Add(new PathSegment { Name = root, Path = root });
                path = path.Substring(root.Length);

                // Add each folder in path
                var parts = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                var currentPath = root;
                foreach (var part in parts)
                {
                    if (!string.IsNullOrEmpty(currentPath))
                    {
                        currentPath = Path.Combine(currentPath, part);
                        segments.Add(new PathSegment { Name = part, Path = currentPath });
                    }
                }

                // Mark last segment
                if (segments.Any())
                    segments.Last().IsLast = true;

                foreach (var segment in segments)
                    PathSegments.Add(segment);
            }
        }

        [RelayCommand]
        private void Refresh()
        {
            if (!string.IsNullOrEmpty(CurrentPath))
            {
                LoadItems(CurrentPath);
            }
        }

        [RelayCommand]
        private void NavigateToSegment(string path)
        {
            if (Directory.Exists(path))
            {
                _backStack.Push(CurrentPath);
                LoadItems(path);
            }
        }

        [RelayCommand]
        private void Sort(string column)
        {
            if (SortColumn == column)
            {
                SortAscending = !SortAscending;
            }
            else
            {
                SortColumn = column;
                SortAscending = true;
            }

            var sorted = Items.ToList();
            IOrderedEnumerable<FileItem> orderedItems;

            // Always sort folders first
            if (SortAscending)
            {
                orderedItems = sorted.OrderByDescending(x => x.IsDirectory).ThenBy(GetSortSelector(column));
            }
            else
            {
                orderedItems = sorted.OrderByDescending(x => x.IsDirectory).ThenByDescending(GetSortSelector(column));
            }

            Items.Clear();
            foreach (var item in orderedItems)
            {
                Items.Add(item);
            }
        }

        private Func<FileItem, IComparable> GetSortSelector(string column)
        {
            return column switch
            {
                "Name" => x => x.Name ?? string.Empty,
                "Type" => x => x.Type,
                "Size" => x => GetSizeForSorting(x),
                "DateModified" => x => DateTime.Parse(x.DateModified),
                _ => x => x.Name ?? string.Empty
            };
        }

        private long GetSizeForSorting(FileItem item)
        {
            if (item.IsDirectory) return -1;
            if (string.IsNullOrEmpty(item.Size)) return 0;

            try
            {
                var parts = item.Size.Split(' ');
                if (parts.Length != 2) return 0;

                var value = double.Parse(parts[0]);
                var unit = parts[1].ToUpper();

                return unit switch
                {
                    "B" => (long)value,
                    "KB" => (long)(value * 1024),
                    "MB" => (long)(value * 1024 * 1024),
                    "GB" => (long)(value * 1024 * 1024 * 1024),
                    "TB" => (long)(value * 1024 * 1024 * 1024 * 1024),
                    _ => 0
                };
            }
            catch
            {
                return 0;
            }
        }

        [RelayCommand]
        private void NavigateBack()
        {
            if (!_backStack.Any() || string.IsNullOrEmpty(CurrentPath)) return;
            _forwardStack.Push(CurrentPath);
            LoadItems(_backStack.Pop());
        }

        [RelayCommand]
        private void NavigateForward()
        {
            if (!_forwardStack.Any() || string.IsNullOrEmpty(CurrentPath)) return;
            _backStack.Push(CurrentPath);
            LoadItems(_forwardStack.Pop());
        }

        [RelayCommand]
        private void NavigateUp()
        {
            if (string.IsNullOrEmpty(CurrentPath)) return;
            var parent = Directory.GetParent(CurrentPath);
            if (parent != null)
            {
                _backStack.Push(CurrentPath);
                LoadItems(parent.FullName);
            }
        }

        [RelayCommand]
        private void NavigateToPath()
        {
            try
            {
                string path = CurrentPath;
                if (Directory.Exists(path))
                {
                    _backStack.Push(CurrentPath);
                    LoadItems(path);
                }
                else
                {
                    System.Windows.MessageBox.Show($"Directory not found: {path}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error navigating to path: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ClearRecentLocations()
        {
            _recentLocations.Clear();
        }

        [RelayCommand]
        private void ItemDoubleClick()
        {
            if (SelectedItem == null) return;

            try
            {
                if (SelectedItem.IsDirectory)
                {
                    _backStack.Push(CurrentPath);
                    LoadItems(SelectedItem.Path!);
                }
                else
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = SelectedItem.Path!,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error opening file: {ex.Message}", "Error",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error accessing path: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ToggleView()
        {
            IsGridView = !IsGridView;
        }

        [RelayCommand]
        private void TogglePathEdit()
        {
            IsEditingPath = !IsEditingPath;
        }

        [RelayCommand]
        private void ToggleSelectionMode()
        {
            IsSelectionMode = !IsSelectionMode;
            System.Diagnostics.Debug.WriteLine($"Selection mode: {IsSelectionMode}");
            if (!IsSelectionMode)
            {
                SelectedItems.Clear();
            }
            OnPropertyChanged(nameof(IsSelectionMode));
        }

        public void SelectAll()
        {
            System.Diagnostics.Debug.WriteLine("Selecting all items...");
            SelectedItems.Clear();
            foreach (var item in Items)
            {
                SelectedItems.Add(item);
            }
            System.Diagnostics.Debug.WriteLine($"Selected {SelectedItems.Count} items");
            
            // If we're selecting all items, automatically enable selection mode
            if (SelectedItems.Count > 0 && !IsSelectionMode)
            {
                IsSelectionMode = true;
            }
        }

        public bool CanDragItems => IsSelectionMode || SelectedItem != null;

        public IEnumerable<string> GetDragPaths()
        {
            if (IsSelectionMode && SelectedItems.Any())
            {
                return SelectedItems.Select(item => item.Path!).Where(path => path != null);
            }
            else if (SelectedItem?.Path != null)
            {
                return new[] { SelectedItem.Path };
            }
            return Array.Empty<string>();
        }

        public void HandleKeyDown(string key, bool controlKey)
        {
            System.Diagnostics.Debug.WriteLine($"Key pressed: {key}, Control: {controlKey}");
            if (controlKey && key == "A")
            {
                SelectAll();
            }
        }

        public void HandlePathKeyDown(string key)
        {
            if (key == "Return")  // WPF uses "Return" for Enter key
            {
                if (Directory.Exists(CurrentPath))
                {
                    _backStack.Push(CurrentPath);
                    LoadItems(CurrentPath);
                    IsEditingPath = false;
                }
                else
                {
                    System.Windows.MessageBox.Show($"Directory not found: {CurrentPath}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            else if (key == "Escape")
            {
                // Restore the original path and exit edit mode
                CurrentPath = _originalPath;
                IsEditingPath = false;
            }
        }

        public void HandlePathLostFocus()
        {
            if (string.IsNullOrWhiteSpace(CurrentPath) || !Directory.Exists(CurrentPath))
            {
                CurrentPath = _originalPath;
            }
            IsEditingPath = false;
        }

        private void LoadItems(string path)
        {
            try
            {
                Items.Clear();
                CurrentPath = path;  // Set the current path
                UpdatePathSegments();  // Update path segments
                AddToRecentLocations(path);  // Add to recent locations when navigating

                foreach (var directory in Directory.GetDirectories(path))
                {
                    var dirInfo = new DirectoryInfo(directory);
                    if ((dirInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        Items.Add(new FileItem
                        {
                            Name = Path.GetFileName(directory),
                            Icon = GetIcon(directory, true),
                            Path = directory,
                            IsDirectory = true,
                            Type = "File folder",
                            Size = "<Folder>",
                            DateModified = dirInfo.LastWriteTime.ToString("g")
                        });
                    }
                }

                foreach (var file in Directory.GetFiles(path))
                {
                    var fileInfo = new FileInfo(file);
                    if ((fileInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        Items.Add(new FileItem
                        {
                            Name = Path.GetFileName(file),
                            Icon = GetIcon(file, false),
                            Path = file,
                            IsDirectory = false,
                            Type = GetFileType(file),
                            Size = FormatFileSize(fileInfo.Length),
                            DateModified = fileInfo.LastWriteTime.ToString("g")
                        });
                    }
                }

                CurrentPath = path;
                UpdatePathSegments();
                OnPropertyChanged(nameof(CanNavigateBack));
                OnPropertyChanged(nameof(CanNavigateForward));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error accessing path: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private static BitmapImage? GetIcon(string path, bool isFolder)
        {
            try
            {
                Icon? icon;
                if (isFolder)
                {
                    using var folderIcon = ExtractIconFromExe(Environment.SystemDirectory + "\\shell32.dll", 3);
                    icon = folderIcon;
                }
                else
                {
                    icon = Icon.ExtractAssociatedIcon(path);
                }

                if (icon == null) return null;

                using var bitmap = icon.ToBitmap();
                using var stream = new MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;

                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze(); // Important for performance
                return image;
            }
            catch
            {
                return null;
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        private static Icon? ExtractIconFromExe(string file, int index)
        {
            var hIcon = ExtractIcon(IntPtr.Zero, file, index);
            if (hIcon == IntPtr.Zero) return null;
            return Icon.FromHandle(hIcon);
        }

        private string GetFileType(string filePath)
        {
            try
            {
                var extension = Path.GetExtension(filePath).ToLower();
                if (string.IsNullOrEmpty(extension))
                    return "File";

                // Get file type description from registry
                var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extension);
                if (key != null)
                {
                    var type = key.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(type))
                    {
                        var typeKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(type);
                        if (typeKey != null)
                        {
                            var desc = typeKey.GetValue(null) as string;
                            if (!string.IsNullOrEmpty(desc))
                                return desc;
                        }
                    }
                }
                return extension.TrimStart('.').ToUpper() + " File";
            }
            catch
            {
                return Path.GetExtension(filePath).ToUpper() + " File";
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        private void AddToRecentLocations(string path)
        {
            // Remove the path if it already exists
            var existingIndex = _recentLocations.IndexOf(path);
            if (existingIndex != -1)
            {
                _recentLocations.RemoveAt(existingIndex);
            }

            // Add to the beginning
            _recentLocations.Insert(0, path);

            // Remove oldest if we exceed the limit
            while (_recentLocations.Count > MAX_RECENT_LOCATIONS)
            {
                _recentLocations.RemoveAt(_recentLocations.Count - 1);
            }

            // Save to disk
            _recentLocationsService.SaveRecentLocations(_recentLocations);

            // Force UI update
            OnPropertyChanged(nameof(RecentLocations));
        }

        partial void OnIsEditingPathChanged(bool value)
        {
            if (value)
            {
                // Entering edit mode - backup the current path
                _originalPath = CurrentPath;
            }
        }

        partial void OnIsSelectionModeChanged(bool value)
        {
            if (!value)
            {
                SelectedItems.Clear();
            }
        }

        public void RefreshCurrentDirectory()
        {
            LoadItems(CurrentPath);
        }
    }

    public class FileItem
    {
        public string? Name { get; set; }
        public BitmapImage? Icon { get; set; }
        public string? Path { get; set; }
        public bool IsDirectory { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string DateModified { get; set; } = string.Empty;
    }

    public class PathSegment
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsLast { get; set; }
    }
} 