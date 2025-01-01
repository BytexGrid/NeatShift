using System.Windows;
using ModernWpf.Controls;
using NeatShift.Services;
using NeatShift.Models;
using System.Runtime.Versioning;

namespace NeatShift.Views
{
    [SupportedOSPlatform("windows7.0")]
    public partial class NeatSavesManagementDialog : ContentDialog
    {
        private readonly INeatSavesService _neatSavesService;

        public NeatSavesManagementDialog(INeatSavesService neatSavesService)
        {
            InitializeComponent();
            _neatSavesService = neatSavesService;
            SavesListView.ItemsSource = _neatSavesService.GetNeatSaves();
        }

        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            var operation = (NeatSavesOperation)button.DataContext;

            var result = MessageBox.Show(
                "Are you sure you want to restore these files? This will overwrite any existing files at the original location.",
                "Confirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                var (success, operationId) = await _neatSavesService.RestoreNeatSave(operation.Id);
                if (success)
                {
                    MessageBox.Show(
                        "Files restored successfully.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    // Prompt to delete the NeatSave
                    var deleteResult = MessageBox.Show(
                        "Would you like to delete this NeatSave now that files have been restored?",
                        "Delete NeatSave",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (deleteResult == MessageBoxResult.Yes)
                    {
                        bool deleteSuccess = await _neatSavesService.DeleteNeatSave(operationId);
                        if (deleteSuccess)
                        {
                            // Refresh the list view
                            SavesListView.ItemsSource = _neatSavesService.GetNeatSaves();
                        }
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Failed to restore files. Please try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            var operation = (NeatSavesOperation)button.DataContext;

            var result = MessageBox.Show(
                "Are you sure you want to delete this backup? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                bool success = await _neatSavesService.DeleteNeatSave(operation.Id);
                if (success)
                {
                    MessageBox.Show(
                        "Backup deleted successfully.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    SavesListView.ItemsSource = _neatSavesService.GetNeatSaves(); // Refresh the list
                }
                else
                {
                    MessageBox.Show(
                        "Failed to delete backup. Please try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }
    }
} 