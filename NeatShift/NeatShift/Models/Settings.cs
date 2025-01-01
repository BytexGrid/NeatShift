using System;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using ModernWpf.Controls;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using CommunityToolkit.Mvvm.Input;

namespace NeatShift.Models
{
    public class Settings : INotifyPropertyChanged
    {
        private bool _createRestorePoint = true;
        private bool _hideSymbolicLinks = true;
        private string _lastUsedPath = string.Empty;
        private bool _useNeatSaves = false;
        private int _maxNeatSaves = 50;
        private string _neatSavesLocation = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NeatShift",
            "NeatSaves"
        );
        private bool _hasShownSafetyChoice = false;

        private ICommand? _browseNeatSavesLocationCommand;
        public ICommand BrowseNeatSavesLocationCommand => _browseNeatSavesLocationCommand ??= new RelayCommand(BrowseNeatSavesLocation);

        private void BrowseNeatSavesLocation()
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = "Select NeatSaves Location",
                IsFolderPicker = true,
                InitialDirectory = NeatSavesLocation
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                NeatSavesLocation = dialog.FileName;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }

        public bool CreateRestorePoint
        {
            get => _createRestorePoint;
            set
            {
                if (_createRestorePoint != value)
                {
                    _createRestorePoint = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HideSymbolicLinks
        {
            get => _hideSymbolicLinks;
            set
            {
                if (_hideSymbolicLinks != value)
                {
                    _hideSymbolicLinks = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LastUsedPath
        {
            get => _lastUsedPath;
            set
            {
                if (_lastUsedPath != value)
                {
                    _lastUsedPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UseNeatSaves
        {
            get => _useNeatSaves;
            set
            {
                if (_useNeatSaves != value)
                {
                    _useNeatSaves = value;
                    OnPropertyChanged();
                }
            }
        }

        public int MaxNeatSaves
        {
            get => _maxNeatSaves;
            set
            {
                if (_maxNeatSaves != value)
                {
                    _maxNeatSaves = value;
                    OnPropertyChanged();
                }
            }
        }

        public string NeatSavesLocation
        {
            get => _neatSavesLocation;
            set
            {
                if (_neatSavesLocation != value)
                {
                    _neatSavesLocation = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasShownSafetyChoice
        {
            get => _hasShownSafetyChoice;
            set
            {
                if (_hasShownSafetyChoice != value)
                {
                    _hasShownSafetyChoice = value;
                    OnPropertyChanged();
                }
            }
        }
    }
} 