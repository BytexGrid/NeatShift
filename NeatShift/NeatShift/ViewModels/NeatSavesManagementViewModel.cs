using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeatShift.Services;
using System.Windows;

namespace NeatShift.ViewModels
{
    public partial class NeatSavesManagementViewModel : ObservableObject
    {
        private readonly INeatSavesService _neatSavesService;
        private readonly string _operationId;

        [ObservableProperty]
        private string _sourcePath;

        [ObservableProperty]
        private string _destinationPath;

        public NeatSavesManagementViewModel(INeatSavesService neatSavesService, string operationId, string sourcePath, string destinationPath)
        {
            _neatSavesService = neatSavesService;
            _operationId = operationId;
            _sourcePath = sourcePath;
            _destinationPath = destinationPath;
        }

        [RelayCommand]
        private async Task RestoreFiles()
        {
            var result = MessageBox.Show(
                "Are you sure you want to restore the files to their original location? This will overwrite any existing files.",
                "Confirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                var (success, operationId) = await _neatSavesService.RestoreNeatSave(_operationId);
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
                            MessageBox.Show(
                                "NeatSave deleted successfully.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information
                            );
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

        [RelayCommand]
        private async Task DeleteBackup()
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this backup? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                bool success = await _neatSavesService.DeleteNeatSave(_operationId);
                if (success)
                {
                    MessageBox.Show(
                        "Backup deleted successfully.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
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

        [RelayCommand]
        private void ManageNeatSaves()
        {
            // TODO: Implement NeatSaves management dialog
            MessageBox.Show(
                "NeatSaves management coming soon!",
                "Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
} 