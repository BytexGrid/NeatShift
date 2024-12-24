using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModernWpf.Controls;
using NeatShift.Models;
using NeatShift.Services;
using NeatShift.Views;
using System.Runtime.Versioning;
using MessageBox = System.Windows.MessageBox;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace NeatShift.ViewModels
{
    [SupportedOSPlatform("windows7.0")]
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IFileOperationService _fileOperationService;
        private readonly ISettingsService _settingsService;

        [ObservableProperty]
        private bool _isOperationInProgress;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private int _progressValue;

        public ObservableCollection<FileSystemItem> SourceItems { get; }

        [ObservableProperty]
        private string _destinationPath = string.Empty;

        public bool CanMove => !IsOperationInProgress;

        [ObservableProperty]
        private bool _isDarkTheme;

        public MainWindowViewModel(IFileOperationService fileOperationService, ISettingsService settingsService)
        {
            _fileOperationService = fileOperationService ?? throw new ArgumentNullException(nameof(fileOperationService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            SourceItems = new ObservableCollection<FileSystemItem>();

            var settings = _settingsService.LoadSettings();
            DestinationPath = settings.LastUsedPath;
            IsDarkTheme = ModernWpf.ThemeManager.Current.ActualApplicationTheme == ModernWpf.ApplicationTheme.Dark;

            _fileOperationService.ProgressChanged += (s, e) =>
            {
                ProgressValue = e.ProgressPercentage;
                StatusMessage = e.Message;
            };
        }

        [RelayCommand]
        private void RemoveItem(FileSystemItem? item)
        {
            if (item != null)
            {
                SourceItems.Remove(item);
            }
        }

        [RelayCommand]
        private async Task OpenSettings()
        {
            var dialog = new SettingsDialog(_settingsService);
            await dialog.ShowAsync();
        }

        [RelayCommand]
        private async Task ViewSymbolicLinks()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select folder to check for symbolic links",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dialog.SelectedPath))
            {
                var linksDialog = new SymbolicLinksDialog(dialog.SelectedPath);
                await linksDialog.ShowAsync();
            }
        }

        partial void OnIsOperationInProgressChanged(bool value)
        {
            AddFilesCommand.NotifyCanExecuteChanged();
            AddFolderCommand.NotifyCanExecuteChanged();
            MoveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanMove));
        }

        [RelayCommand(CanExecute = nameof(CanExecuteFileOperations))]
        private void AddFiles()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Title = "Select files to move"
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (string file in dialog.FileNames)
                {
                    if (!string.IsNullOrEmpty(file))
                    {
                        SourceItems.Add(new FileSystemItem(file));
                    }
                }
            }
        }

        [RelayCommand(CanExecute = nameof(CanExecuteFileOperations))]
        private void AddFolder()
        {
            using var dialog = new CommonOpenFileDialog
            {
                Title = "Select folders to move",
                IsFolderPicker = true,
                Multiselect = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                foreach (string folder in dialog.FileNames)
                {
                    if (!string.IsNullOrEmpty(folder))
                    {
                        SourceItems.Add(new FileSystemItem(folder));
                    }
                }
            }
        }

        [RelayCommand]
        private void BrowseDestination()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select destination folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    DestinationPath = dialog.SelectedPath;
                    var settings = _settingsService.LoadSettings();
                    settings.LastUsedPath = dialog.SelectedPath;
                    _settingsService.SaveSettings(settings);
                }
            }
        }

        [RelayCommand(CanExecute = nameof(CanExecuteFileOperations))]
        private async Task Move()
        {
            if (string.IsNullOrEmpty(DestinationPath))
            {
                MessageBox.Show("Please select a destination folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                IsOperationInProgress = true;
                StatusMessage = "Moving files...";
                ProgressValue = 0;

                var items = SourceItems.ToList();
                foreach (var item in items)
                {
                    string destinationPath = Path.Combine(DestinationPath, Path.GetFileName(item.Path));
                    bool success = await _fileOperationService.MoveWithSymbolicLink(item.Path, destinationPath);
                    if (success)
                    {
                        SourceItems.Remove(item);
                    }
                }

                if (SourceItems.Count == 0)
                {
                    StatusMessage = "Operation completed successfully";
                    ProgressValue = 100;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsOperationInProgress = false;
            }
        }

        private bool CanExecuteFileOperations() => !IsOperationInProgress;

        public void AddSourceItem(string path)
        {
            var item = new FileSystemItem(path);
            if (!SourceItems.Any(x => x.Path == item.Path))
            {
                SourceItems.Add(item);
            }
        }

        [RelayCommand]
        private void OpenGitHub()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/BytexGrid/NeatShift",
                UseShellExecute = true
            });
        }

        [RelayCommand]
        private async Task ShowAbout()
        {
            var dialog = new ContentDialog
            {
                Title = "About NeatShift",
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close,
                Content = new TextBlock
                {
                    Text = "NeatShift v1.1.0\n\n" +
                           "A modern file organization tool with symbolic link support.\n\n" +
                           "Features:\n" +
                           "• Move files and folders while keeping them accessible\n" +
                           "• Create and manage symbolic links\n" +
                           "• System restore point creation\n" +
                           "• Modern Windows 11 style interface\n\n" +
                           "⚠️ Important Notice:\n" +
                           "NeatShift is currently in testing phase. While we implement safety measures, please:\n" +
                           "• Create manual backups or system restore points\n" +
                           "• Verify symbolic links after moving files\n" +
                           "• Report any issues through Discord or Telegram\n\n" +
                           "The software is provided 'as is', without warranty of any kind.\n\n" +
                           "© 2024 NeatShift",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 10)
                }
            };

            await dialog.ShowAsync();
        }

        [RelayCommand]
        private async Task ShowContactDialog()
        {
            var dialog = new ContactDialog();
            await dialog.ShowAsync();
        }

        [RelayCommand]
        private async Task ShowFeatureRequest()
        {
            var dialog = new FeatureRequestDialog();
            await dialog.ShowAsync();
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            ModernWpf.ThemeManager.Current.ApplicationTheme = IsDarkTheme 
                ? ModernWpf.ApplicationTheme.Dark 
                : ModernWpf.ApplicationTheme.Light;
        }
    }
} 