using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using NeatShift.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Windows.Data;
using System.Threading.Tasks;

namespace NeatShift.Views
{
    public partial class FileBrowserControl : UserControl
    {
        private Point _startPoint;
        private FileBrowserViewModel? ViewModel => DataContext as FileBrowserViewModel;

        public FileBrowserControl()
        {
            InitializeComponent();
            DataContext = ((App)App.Current).Services.GetRequiredService<FileBrowserViewModel>();
            
            // Enable drag on the ListView
            FileListView.PreviewMouseLeftButtonDown += FileListView_PreviewMouseLeftButtonDown;
            FileListView.PreviewMouseMove += FileListView_PreviewMouseMove;
            FileListView.PreviewMouseLeftButtonUp += FileListView_PreviewMouseLeftButtonUp;
            FileListView.Drop += FileListView_Drop;
            FileListView.DragEnter += FileListView_DragEnter;

            // Add key handlers to both views
            FileListView.PreviewKeyDown += ListView_KeyDown;
            this.PreviewKeyDown += ListView_KeyDown; // Handle keys at UserControl level

            PathTextBox.LostFocus += PathTextBox_LostFocus;
        }

        private void FileListView_Drop(object sender, DragEventArgs e)
        {
            // Placeholder for future implementation
        }

        private void FileListView_DragEnter(object sender, DragEventArgs e)
        {
            // Placeholder for future implementation
        }

        private void FileBrowserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel?.Initialize();
        }

        private async void FileListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Services.AdminManager.IsAdminGranted)
            {
                var (reason, actions) = NeatShift.Services.AdminManager.Messages.SymbolicLink;
                bool granted = await NeatShift.Services.AdminManager.EnsureAdmin(reason, actions);
                if (!granted)
                {
                    // user cancelled; abort drag initiate
                    return;
                }
                // App will relaunch if granted, so this process ends
            }
            _startPoint = e.GetPosition(null);
        }

        private void FileListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && 
                ViewModel != null && 
                ViewModel.CanDragItems)
            {
                var paths = ViewModel.GetDragPaths().ToArray();
                if (paths.Length > 0)
                {
                    var data = new DataObject(DataFormats.FileDrop, paths);
                    DragDrop.DoDragDrop(FileListView, data, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }
        }

        private void FileListView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // No longer need to track dragging state
        }

        private void StartDrag()
        {
            if (ViewModel != null)
            {
                var selectedItems = ViewModel.SelectedItems.ToList();
                if (selectedItems.Count > 0)
                {
                    var filePaths = selectedItems.Select(item => item.Path).ToArray();
                    var dataObject = new DataObject(DataFormats.FileDrop, filePaths);
                    DragDrop.DoDragDrop(FileListView, dataObject, DragDropEffects.Copy);
                }
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.ItemDoubleClickCommand.Execute(null);
            }
        }

        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel != null)
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    ViewModel.ItemDoubleClickCommand.Execute(null);
                }
                else if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = true;
                    ViewModel.HandleKeyDown("A", true);
                }
            }
        }

        private void ColumnHeader_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && ViewModel != null)
            {
                ViewModel.SortCommand.Execute(textBlock.Tag?.ToString());
            }
        }

        private void PathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel != null)
            {
                if (e.Key == Key.Enter || e.Key == Key.Escape)
                {
                    e.Handled = true;
                    ViewModel.HandlePathKeyDown(e.Key.ToString());
                }
            }
        }

        private void Breadcrumb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Only handle clicks on the background, not on the buttons
            if (e.Source is Border && ViewModel != null)
            {
                ViewModel.IsEditingPath = true;
                e.Handled = true;
                
                // Focus the TextBox and select all text
                PathTextBox.Focus();
                PathTextBox.SelectAll();
            }
        }

        private void QuickAccess_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null && ViewModel != null)
            {
                // Clear existing location items
                var menu = button.ContextMenu;
                var itemsToRemove = menu.Items.Cast<object>()
                    .Where(item => item is MenuItem menuItem && 
                           menuItem.Header?.ToString() != "Recent Locations" && 
                           menuItem.Header?.ToString() != "Clear Recent Locations")
                    .ToList();

                foreach (var item in itemsToRemove)
                {
                    menu.Items.Remove(item);
                }

                // Add recent locations after the header
                var headerIndex = menu.Items.Cast<object>()
                    .TakeWhile(item => item is MenuItem menuItem && 
                             menuItem.Header?.ToString() != "Recent Locations")
                    .Count();

                var insertIndex = headerIndex + 2; // After header and separator

                foreach (var location in ViewModel.RecentLocations)
                {
                    var menuItem = new MenuItem
                    {
                        Header = location,
                        Command = ViewModel.NavigateToPathCommand,
                        CommandParameter = location
                    };

                    var icon = new TextBlock
                    {
                        Text = "\uE8DA",
                        FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"),
                        FontSize = 14,
                        Foreground = System.Windows.Application.Current.Resources["SystemControlForegroundBaseMediumBrush"] as System.Windows.Media.Brush
                    };
                    System.Windows.Automation.AutomationProperties.SetName(icon, "Folder Icon");

                    menuItem.Icon = icon;
                    menu.Items.Insert(insertIndex++, menuItem);
                }

                button.ContextMenu.IsOpen = true;
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel != null && ViewModel.IsSelectionMode)
            {
                System.Diagnostics.Debug.WriteLine($"Selection changed - Added: {e.AddedItems.Count}, Removed: {e.RemovedItems.Count}");
                ViewModel.SelectedItems.Clear();
                foreach (FileItem item in FileListView.SelectedItems)
                {
                    ViewModel.SelectedItems.Add(item);
                }
                System.Diagnostics.Debug.WriteLine($"Total selected items: {ViewModel.SelectedItems.Count}");
            }
        }

        private void SelectAllItems()
        {
            if (ViewModel != null)
            {
                ViewModel.SelectAll();
            }
        }

        private void PathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel?.HandlePathLostFocus();
        }
    }
} 