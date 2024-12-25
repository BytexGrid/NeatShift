using System.Runtime.Versioning;
using System.Windows;
using NeatShift.ViewModels;
using System.Windows.Documents;

namespace NeatShift.Views
{
    [SupportedOSPlatform("windows7.0")]
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
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
    }
} 