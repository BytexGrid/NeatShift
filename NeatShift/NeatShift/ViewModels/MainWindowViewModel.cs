using System;
using System.Collections.Generic;
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
        private readonly INeatSavesService _neatSavesService;
        private readonly FileBrowserViewModel _fileBrowserViewModel;
        private string _sourceDirectory = string.Empty;

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

        private bool _isGuideSelected = true;
        
        public bool IsGuideSelected
        {
            get => _isGuideSelected;
            set
            {
                SetProperty(ref _isGuideSelected, value);
                OnPropertyChanged(nameof(IsExplorerSelected));
            }
        }

        public bool IsExplorerSelected => !_isGuideSelected;

        [RelayCommand]
        private void SwitchView(string view)
        {
            IsGuideSelected = view == "Guide";
        }

        public MainWindowViewModel(IFileOperationService fileOperationService, ISettingsService settingsService, INeatSavesService neatSavesService, FileBrowserViewModel fileBrowserViewModel)
        {
            _fileOperationService = fileOperationService ?? throw new ArgumentNullException(nameof(fileOperationService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _neatSavesService = neatSavesService ?? throw new ArgumentNullException(nameof(neatSavesService));
            SourceItems = new ObservableCollection<FileSystemItem>();

            // Initialize NeatSavesService
            Task.Run(async () =>
            {
                await _neatSavesService.Initialize();
            }).ConfigureAwait(false);

            var settings = _settingsService.LoadSettings();
            DestinationPath = settings.LastUsedPath;
            IsDarkTheme = ModernWpf.ThemeManager.Current.ActualApplicationTheme == ModernWpf.ApplicationTheme.Dark;

            _fileOperationService.ProgressChanged += (s, e) =>
            {
                ProgressValue = e.ProgressPercentage;
                StatusMessage = e.Message;
            };

            _fileBrowserViewModel = fileBrowserViewModel;
            SourceItems.CollectionChanged += (s, e) =>
            {
                if (e.NewItems?.Count > 0)
                {
                    // Store the directory of the first added item as our source directory
                    var firstItem = e.NewItems[0] as string;
                    if (!string.IsNullOrEmpty(firstItem))
                    {
                        _sourceDirectory = Path.GetDirectoryName(firstItem) ?? string.Empty;
                    }
                }
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
                var (reason, actions) = AdminManager.Messages.ViewLinks;
                if (await AdminManager.EnsureAdmin(reason, actions))
                {
                    var linksDialog = new SymbolicLinksDialog(dialog.SelectedPath);
                    await linksDialog.ShowAsync();
                }
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
        private async Task AddFiles()
        {
            var (reason, actions) = AdminManager.Messages.SymbolicLink;
            if (await AdminManager.EnsureAdmin(reason, actions))
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
        }

        [RelayCommand(CanExecute = nameof(CanExecuteFileOperations))]
        private async Task AddFolder()
        {
            var (reason, actions) = AdminManager.Messages.SymbolicLink;
            if (await AdminManager.EnsureAdmin(reason, actions))
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
            if (!SourceItems.Any())
            {
                MessageBox.Show(
                    "The source files list is empty.\n\n" +
                    "You can add files by:\n" +
                    "• Dragging files from the in-app explorer\n" +
                    "• Using the 'Add Files' button\n" +
                    "• Using the 'Add Folder' button\n" +
                    "• Dragging files from Windows Explorer",
                    "No Files to Move",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return;
            }

            if (string.IsNullOrEmpty(DestinationPath))
            {
                MessageBox.Show("Please select a destination folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Show safety choice dialog if it's the first time
            var settings = _settingsService.LoadSettings();
            if (!settings.HasShownSafetyChoice)
            {
                var safetyDialog = new SafetyChoiceDialog();
                if (safetyDialog.ShowDialog() != true)
                {
                    return; // User cancelled
                }

                // Save user's choices
                settings.UseNeatSaves = safetyDialog.ViewModel.UseNeatSavesOnly || safetyDialog.ViewModel.UseBoth;
                settings.CreateRestorePoint = safetyDialog.ViewModel.UseSystemRestoreOnly || safetyDialog.ViewModel.UseBoth;
                
                if (safetyDialog.ViewModel.RememberChoice)
                {
                    settings.HasShownSafetyChoice = true;
                }
                
                // Always save settings regardless of remember choice
                _settingsService.SaveSettings(settings);
            }

            // Show confirmation dialog
            var result = MessageBox.Show(
                $"Are you sure you want to move {SourceItems.Count} item(s) to:\n{DestinationPath}?",
                "Confirm Move",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // Request admin rights if needed
            var (reason, actions) = AdminManager.Messages.SymbolicLink;
            if (await AdminManager.EnsureAdmin(reason, actions))
            {
                await MoveFiles();
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
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = $"NeatShift v{Version.Current}\n\n" +
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
                        },
                        new System.Windows.Controls.Button
                        {
                            Content = "Check for Updates",
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            Command = new RelayCommand(async () => await CheckForUpdates())
                        }
                    }
                }
            };

            await dialog.ShowAsync();
        }

        private async Task CheckForUpdates()
        {
            var updateService = new UpdateService();
            var (isUpdateAvailable, latestVersion) = await updateService.CheckForUpdates();
            
            if (isUpdateAvailable)
            {
                var result = MessageBox.Show(
                    $"A new version ({latestVersion}) is available. Would you like to download it?",
                    "Update Available",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/BytexGrid/NeatShift/releases/latest",
                        UseShellExecute = true
                    });
                }
            }
            else
            {
                MessageBox.Show(
                    "You are using the latest version.",
                    "No Updates Available",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
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
        private async Task ManageRestorePoints()
        {
            var (reason, actions) = AdminManager.Messages.Backup;
            if (await AdminManager.EnsureAdmin(reason, actions))
            {
                var dialog = new RestorePointDialog();
                await dialog.ShowAsync();
            }
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            ModernWpf.ThemeManager.Current.ApplicationTheme = IsDarkTheme 
                ? ModernWpf.ApplicationTheme.Dark 
                : ModernWpf.ApplicationTheme.Light;
        }

        [RelayCommand]
        private void ShowSafetyChoice()
        {
            var safetyDialog = new SafetyChoiceDialog();
            if (safetyDialog.ShowDialog() == true)
            {
                var settings = _settingsService.LoadSettings();
                settings.UseNeatSaves = safetyDialog.ViewModel.UseNeatSavesOnly || safetyDialog.ViewModel.UseBoth;
                settings.CreateRestorePoint = safetyDialog.ViewModel.UseSystemRestoreOnly || safetyDialog.ViewModel.UseBoth;
                
                if (safetyDialog.ViewModel.RememberChoice)
                {
                    settings.HasShownSafetyChoice = true;
                }
                
                _settingsService.SaveSettings(settings);
            }
        }

        partial void OnStatusMessageChanged(string value)
        {
            if (value == "Operation completed successfully")
            {
                // If we're in the source directory, refresh it
                if (_fileBrowserViewModel.CurrentPath == _sourceDirectory)
                {
                    _fileBrowserViewModel.RefreshCurrentDirectory();
                }
                // Clear the source directory after operation completes
                _sourceDirectory = string.Empty;
            }
        }

        [ObservableProperty]
        private NeatSavesManagementViewModel? _neatSavesManagement;

        [ObservableProperty]
        private bool _showNeatSavesManagement;

        private async Task MoveFiles()
        {
            IsOperationInProgress = true;
            ProgressValue = 0;
            StatusMessage = "Starting operation...";

            try
            {
                if (string.IsNullOrEmpty(DestinationPath))
                {
                    MessageBox.Show("Please select a destination folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!SourceItems.Any())
                {
                    MessageBox.Show("Please add files or folders to move.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var settings = _settingsService.LoadSettings();
                bool success = true;
                var movedItems = new List<string>();

                foreach (var item in SourceItems.ToList())
                {
                    string destinationPath = Path.Combine(DestinationPath, Path.GetFileName(item.Path));
                    bool itemSuccess = await _fileOperationService.MoveWithSymbolicLink(item.Path, destinationPath);
                    if (itemSuccess)
                    {
                        movedItems.Add(item.Path);
                    }
                    else
                    {
                        success = false;
                    }
                }

                if (success)
                {
                    StatusMessage = "Operation completed successfully";
                    ProgressValue = 100;

                    // Create NeatSave if enabled and files were moved
                    if (settings.UseNeatSaves && movedItems.Any())
                    {
                        var neatSaveId = Guid.NewGuid().ToString();
                        var description = $"Files moved from {_sourceDirectory} to {DestinationPath}";
                        try
                        {
                            bool neatSaveSuccess = await _neatSavesService.CreateNeatSave(
                                string.Join(";", movedItems),  // Store all source paths
                                DestinationPath,
                                description
                            );

                            if (neatSaveSuccess)
                            {
                                var neatSavesDialog = new NeatSavesManagementDialog(_neatSavesService);
                                await neatSavesDialog.ShowAsync();
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Files were moved successfully but creating NeatSave backup failed.\n\n" +
                                    "Possible causes:\n" +
                                    "• Insufficient disk space in NeatSaves location\n" +
                                    "• No write permissions to NeatSaves folder\n" +
                                    "• NeatSaves folder path is invalid\n\n" +
                                    "You can check the NeatSaves settings in Settings > NeatSaves Options",
                                    "Warning",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Files were moved successfully but creating NeatSave backup failed.\n\n" +
                                $"Error details: {ex.Message}\n\n" +
                                "You can check the NeatSaves settings in Settings > NeatSaves Options",
                                "Warning",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning
                            );
                        }
                    }

                    // Clear source items after successful operation
                    SourceItems.Clear();
                }
                else
                {
                    StatusMessage = "Operation failed";
                    ProgressValue = 0;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                ProgressValue = 0;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsOperationInProgress = false;
            }
        }

        [RelayCommand]
        private async Task ManageNeatSaves()
        {
            var (reason, actions) = AdminManager.Messages.Backup;
            if (await AdminManager.EnsureAdmin(reason, actions))
            {
                var dialog = new NeatSavesManagementDialog(_neatSavesService);
                await dialog.ShowAsync();
            }
        }

        [RelayCommand]
        private void ChangeSafetyOptions()
        {
            var safetyDialog = new SafetyChoiceDialog();
            if (safetyDialog.ShowDialog() == true)
            {
                var settings = _settingsService.LoadSettings();
                settings.UseNeatSaves = safetyDialog.ViewModel.UseNeatSavesOnly || safetyDialog.ViewModel.UseBoth;
                settings.CreateRestorePoint = safetyDialog.ViewModel.UseSystemRestoreOnly || safetyDialog.ViewModel.UseBoth;
                
                if (safetyDialog.ViewModel.RememberChoice)
                {
                    settings.HasShownSafetyChoice = true;
                }
                
                _settingsService.SaveSettings(settings);
            }
        }
    }
} 