using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using ModernWpf.Controls;
using NeatShift.Services;
using System.Runtime.Versioning;

namespace NeatShift.Views
{
    [SupportedOSPlatform("windows7.0")]
    public partial class SymbolicLinksDialog : ContentDialog
    {
        private readonly string _rootPath;
        private readonly ObservableCollection<SymbolicLinkItem> _links;

        public SymbolicLinksDialog(string rootPath)
        {
            InitializeComponent();
            _rootPath = rootPath;
            _links = new ObservableCollection<SymbolicLinkItem>();
            LinksListView.ItemsSource = _links;

            ShowHiddenLinksCheckBox.Checked += (s, e) => RefreshLinks();
            ShowHiddenLinksCheckBox.Unchecked += (s, e) => RefreshLinks();
            ShowSubdirectoriesCheckBox.Checked += (s, e) => RefreshLinks();
            ShowSubdirectoriesCheckBox.Unchecked += (s, e) => RefreshLinks();

            RefreshLinks();
        }

        [SupportedOSPlatform("windows")]
        private void RefreshLinks()
        {
            _links.Clear();
            var links = ShowSubdirectoriesCheckBox.IsChecked == true
                ? IOHelper.GetAllSymbolicLinks(_rootPath)
                : IOHelper.GetSymbolicLinks(_rootPath);

            foreach (var (path, target, isHidden) in links)
            {
                if (!ShowHiddenLinksCheckBox.IsChecked == true && isHidden)
                    continue;

                _links.Add(new SymbolicLinkItem
                {
                    Path = path,
                    Name = System.IO.Path.GetFileName(path),
                    Target = target,
                    Status = isHidden ? "Hidden" : "Visible"
                });
            }

            UpdateSelectionStatus();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshLinks();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (SelectAllCheckBox.IsChecked == true)
            {
                LinksListView.SelectAll();
            }
            else
            {
                LinksListView.UnselectAll();
            }
        }

        private void LinksListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateSelectionStatus();
        }

        private void UpdateSelectionStatus()
        {
            int selectedCount = LinksListView.SelectedItems.Count;
            SelectedCountText.Text = $"{selectedCount} item{(selectedCount == 1 ? "" : "s")} selected";

            DeleteButton.IsEnabled = selectedCount > 0;
            ToggleVisibilityButton.IsEnabled = selectedCount > 0;
            ShowInExplorerButton.IsEnabled = selectedCount == 1;

            SelectAllCheckBox.IsChecked = selectedCount == _links.Count;
            SelectAllCheckBox.IsEnabled = _links.Count > 0;
        }

        [SupportedOSPlatform("windows")]
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = LinksListView.SelectedItems.Cast<SymbolicLinkItem>().ToList();
            if (!selectedItems.Any()) return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete {selectedItems.Count} symbolic link{(selectedItems.Count == 1 ? "" : "s")}? " +
                    $"This will not delete the target file{(selectedItems.Count == 1 ? "" : "s")}/folder{(selectedItems.Count == 1 ? "" : "s")}.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    bool anySuccess = false;
                    var failedItems = new List<string>();

                    foreach (var item in selectedItems)
                    {
                        try
                        {
                            if (IOHelper.DeleteSymbolicLink(item.Path))
                            {
                                anySuccess = true;
                            }
                            else
                            {
                                failedItems.Add(item.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to delete {item.Name}: {ex.Message}");
                            failedItems.Add(item.Name);
                        }
                    }

                    // Refresh the list first to show the current state
                    RefreshLinks();

                    // Show a single message with the results
                    if (failedItems.Any())
                    {
                        var message = anySuccess ? 
                            $"Some symbolic links were deleted successfully, but the following failed:\n{string.Join("\n", failedItems)}" :
                            $"Failed to delete the following symbolic links:\n{string.Join("\n", failedItems)}";

                        MessageBox.Show(
                            message,
                            "Operation Completed with Errors",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show(
                            "All selected symbolic links were deleted successfully.",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Delete_Click: {ex.Message}");
                MessageBox.Show(
                    "An error occurred while deleting symbolic links. Please try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [SupportedOSPlatform("windows")]
        private void ToggleVisibility_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = LinksListView.SelectedItems.Cast<SymbolicLinkItem>().ToList();
            if (!selectedItems.Any()) return;

            bool anyFailure = false;
            foreach (var item in selectedItems)
            {
                if (!IOHelper.ToggleSymbolicLinkVisibility(item.Path))
                {
                    anyFailure = true;
                }
            }

            if (anyFailure)
            {
                var result = MessageBox.Show(
                    "Failed to change visibility for one or more symbolic links. This operation requires administrator privileges.\n\n" +
                    "Would you like to restart the application as administrator?",
                    "Administrator Privileges Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Get the path to the current executable
                    var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                    if (exePath != null)
                    {
                        try
                        {
                            // Start the process with admin privileges
                            var startInfo = new ProcessStartInfo
                            {
                                FileName = exePath,
                                UseShellExecute = true,
                                Verb = "runas" // This requests elevation
                            };
                            Process.Start(startInfo);

                            // Close the current instance
                            Application.Current.Shutdown();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Failed to restart as administrator: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
            }

            RefreshLinks();
        }

        [SupportedOSPlatform("windows")]
        private void ShowInExplorer_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = LinksListView.SelectedItem as SymbolicLinkItem;
            if (selectedItem == null) return;

            try
            {
                Process.Start("explorer.exe", $"/select,\"{selectedItem.Path}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open Explorer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class SymbolicLinkItem
    {
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
} 