using System.Runtime.Versioning;
using System.Windows;
using NeatShift.ViewModels;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NeatShift.Views
{
    [SupportedOSPlatform("windows7.0")]
    public partial class MainWindow : Window
    {
        private readonly FileBrowserViewModel _fileBrowserViewModel;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _fileBrowserViewModel = ((App)App.Current).Services.GetRequiredService<FileBrowserViewModel>();
            
            // Subscribe to status message changes to detect operation completion
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainWindowViewModel.StatusMessage) && 
                    viewModel.StatusMessage == "Operation completed successfully")
                {
                    _fileBrowserViewModel.RefreshCurrentDirectory();
                }
            };
        }

        private async void SymbolicLink_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SymbolicLinkInfoDialog();
            await dialog.ShowAsync();
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files != null && files.Length > 0)
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