using System.Runtime.Versioning;
using System.Windows;
using NeatShift.ViewModels;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NeatShift.Services;

namespace NeatShift.Views
{
    [SupportedOSPlatform("windows7.0")]
    public partial class MainWindow : Window
    {
        private readonly FileBrowserViewModel _fileBrowserViewModel;
        private readonly ISettingsService _settingsService;

        public MainWindow(MainWindowViewModel viewModel, ISettingsService settingsService)
        {
            InitializeComponent();
            DataContext = viewModel;
            _fileBrowserViewModel = ((App)App.Current).Services.GetRequiredService<FileBrowserViewModel>();
            _settingsService = settingsService;
            
            // Subscribe to status message changes to detect operation completion
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainWindowViewModel.StatusMessage) && 
                    viewModel.StatusMessage == "Operation completed successfully")
                {
                    _fileBrowserViewModel.RefreshCurrentDirectory();
                }
            };

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = _settingsService.LoadSettings();

            if (settings.IsFirstLaunch)
            {
                this.WindowState = WindowState.Maximized;
                settings.IsFirstLaunch = false;
                _settingsService.SaveSettings(settings);
            }
            else
            {
                this.Height = settings.WindowHeight;
                this.Width = settings.WindowWidth;
                this.Top = settings.WindowTop;
                this.Left = settings.WindowLeft;
                this.WindowState = settings.WindowState;
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            var settings = _settingsService.LoadSettings();
            settings.WindowState = this.WindowState;
            // Only save size and position if the window is in a normal state
            if (this.WindowState == WindowState.Normal)
            {
                settings.WindowHeight = this.Height;
                settings.WindowWidth = this.Width;
                settings.WindowTop = this.Top;
                settings.WindowLeft = this.Left;
            }
            _settingsService.SaveSettings(settings);
        }

        private async void SymbolicLink_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SymbolicLinkInfoDialog();
            await dialog.ShowAsync();
        }

        private async void ListView_Drop(object sender, DragEventArgs e)
        {
            try
            {
                // If not elevated, intercept the drop, save state, and relaunch as admin.
                if (!AdminManager.IsAdminGranted && e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[]? droppedFiles = e.Data.GetData(DataFormats.FileDrop) as string[];
                    if (droppedFiles?.Length > 0)
                    {
                        var vm = (MainWindowViewModel)DataContext;
                        var existingFiles = vm.SourceItems.Select(i => i.Path!).ToList();
                        var allFilePaths = existingFiles.Concat(droppedFiles).Distinct().ToList();

                        var pendingState = new PendingMove
                        {
                            DestinationPath = vm.DestinationPath,
                            SourcePaths = allFilePaths
                        };
                        PendingMoveManager.Save(pendingState);

                        var (reason, actions) = AdminManager.Messages.SymbolicLink;
                        await AdminManager.EnsureAdmin(reason, actions);
                        return; // App will restart if elevation is granted.
                    }
                }

                // This code path is for when the app is already elevated.
                // Note: Windows UIPI will block drops from unelevated apps like Explorer.
                // This will only work if the source is also elevated.
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[]? files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files?.Length > 0)
                    {
                        var vm = (MainWindowViewModel)DataContext;
                        foreach (string file in files)
                        {
                            vm.AddSourceItem(file);
                        }
                    }
                }
            }
            finally
            {
                e.Handled = true;
            }
        }

        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ListView_DragLeave(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void ViewSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && DataContext is MainWindowViewModel viewModel)
            {
                viewModel.IsGuideSelected = comboBox.SelectedIndex == 0;
            }
        }

        private async void SymbolicLinks_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SymbolicLinkInfoDialog();
            await dialog.ShowAsync();
        }
    }
} 