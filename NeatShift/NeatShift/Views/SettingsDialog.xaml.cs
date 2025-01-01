using ModernWpf.Controls;
using NeatShift.Models;
using NeatShift.Services;
using System.Runtime.Versioning;
using System.Windows.Forms;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NeatShift.Views
{
    [SupportedOSPlatform("windows7.0")]
    public partial class SettingsDialog : ContentDialog, INotifyPropertyChanged
    {
        private readonly Settings _settings;
        private readonly ISettingsService _settingsService;
        public ICommand BrowseNeatSavesLocationCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }

        public SettingsDialog(ISettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _settings = settingsService.LoadSettings();
            DataContext = _settings;

            BrowseNeatSavesLocationCommand = new RelayCommand(BrowseNeatSavesLocation);
            this.PrimaryButtonClick += SettingsDialog_PrimaryButtonClick;
        }

        private void BrowseNeatSavesLocation()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select NeatSaves Location",
                UseDescriptionForTitle = true,
                SelectedPath = _settings.NeatSavesLocation
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _settings.NeatSavesLocation = dialog.SelectedPath;
                OnPropertyChanged(nameof(_settings.NeatSavesLocation));
            }
        }

        private void SettingsDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _settingsService.SaveSettings(_settings);
        }
    }
} 