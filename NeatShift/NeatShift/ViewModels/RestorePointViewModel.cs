//    NeatShift - Easily move files while keeping them accessible
//    Copyright (C) 2024 BytexGrid
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeatShift.Models;

namespace NeatShift.ViewModels
{
    public partial class RestorePointViewModel : ObservableObject
    {
        private readonly Services.ISystemRestoreService _systemRestoreService;
        
        [ObservableProperty]
        private ObservableCollection<RestorePoint> _restorePoints = new();

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (SetProperty(ref _statusMessage, value))
                {
                    OnPropertyChanged(nameof(HasStatusMessage));
                }
            }
        }

        [ObservableProperty]
        private bool _isLoading = false;

        public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

        public bool HasRestorePoints => RestorePoints.Count > 0;

        public RestorePointViewModel()
        {
            _systemRestoreService = new Services.SystemRestoreService();
            LoadRestorePointsAsync().ConfigureAwait(false);
        }

        [RelayCommand]
        private async Task Refresh()
        {
            IsLoading = true;
            try
            {
                await LoadRestorePointsAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadRestorePointsAsync()
        {
            try
            {
                var points = await _systemRestoreService.GetRestorePoints();
                RestorePoints.Clear();
                foreach (var point in points.OrderByDescending(p => p.CreationTime))
                {
                    RestorePoints.Add(point);
                }

                if (RestorePoints.Count == 0)
                {
                    StatusMessage = "No restore points found. Click the 'Create New Restore Point' button above.";
                }
                else
                {
                    StatusMessage = string.Empty;
                }

                OnPropertyChanged(nameof(HasRestorePoints));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading restore points: {ex.Message}");
                StatusMessage = "Could not load restore points. Please run as administrator.";
            }
        }

        [RelayCommand]
        private async Task CreateRestorePoint()
        {
            IsLoading = true;
            StatusMessage = string.Empty;

            try
            {
                // Get current points before trying to create
                var beforePoints = await _systemRestoreService.GetRestorePoints();
                var success = await _systemRestoreService.CreateRestorePoint("Manual restore point from NeatShift");
                
                if (!success)
                {
                    // Get points again to check if any were created in the last 3 minutes
                    var afterPoints = await _systemRestoreService.GetRestorePoints();
                    var recentPoint = afterPoints.FirstOrDefault();
                    var timeSinceLastPoint = recentPoint != null ? 
                        (DateTime.Now - recentPoint.CreationTime).TotalMinutes : 
                        double.MaxValue;

                    if (timeSinceLastPoint < 3)
                    {
                        StatusMessage = "Please wait at least 3 minutes between creating restore points. " +
                                      "This limit helps prevent system resource abuse.";
                    }
                    else
                    {
                        StatusMessage = "Failed to create restore point. Please ensure:\n" +
                                      "1. NeatShift is running as administrator\n" +
                                      "2. System Protection is enabled\n" +
                                      "3. You have enough disk space";
                    }
                }
                else
                {
                    await Refresh();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating restore point: {ex.Message}");
                StatusMessage = "An unexpected error occurred while creating the restore point.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void OpenSystemProtection()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "SystemPropertiesProtection.exe",
                    UseShellExecute = true,
                    Verb = "runas" // Request admin privileges
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening System Protection: {ex.Message}");
                MessageBox.Show(
                    "Could not open System Protection settings. Please open Windows Settings and search for 'System Protection'.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task RestoreToPoint(RestorePoint point)
        {
            var result = MessageBox.Show(
                "Are you sure you want to restore your system to this point? Your computer will restart.",
                "Confirm System Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        // Launch the system restore UI with admin privileges
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "rstrui.exe",
                            UseShellExecute = true,
                            Verb = "runas"
                        });
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error starting system restore: {ex.Message}");
                    MessageBox.Show(
                        "Could not start System Restore. Please ensure you have administrator privileges.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task DeleteRestorePoint(RestorePoint point)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this restore point? This cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    bool success = await _systemRestoreService.DeleteRestorePoint(point.Id);
                    if (success)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            RestorePoints.Remove(point);
                        });

                        MessageBox.Show(
                            "Restore point deleted successfully.",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        await Refresh(); // Refresh the list to ensure it's up to date
                    }
                    else
                    {
                        MessageBox.Show(
                            "Could not delete restore point. Please ensure you have administrator privileges.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deleting restore point: {ex.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    MessageBox.Show(
                        "Could not delete restore point. Please ensure you have administrator privileges.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }
} 