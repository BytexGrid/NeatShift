using ModernWpf.Controls;
using NeatShift.Models;
using NeatShift.Services;
using System.Runtime.Versioning;

namespace NeatShift.Views
{
    [SupportedOSPlatform("windows7.0")]
    public partial class SettingsDialog : ContentDialog
    {
        private readonly Settings _settings;
        private readonly ISettingsService _settingsService;

        public SettingsDialog(ISettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _settings = settingsService.LoadSettings();
            DataContext = _settings;

            this.PrimaryButtonClick += SettingsDialog_PrimaryButtonClick;
        }

        private void SettingsDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _settingsService.SaveSettings(_settings);
        }
    }
} 