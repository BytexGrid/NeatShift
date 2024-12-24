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
    }
} 