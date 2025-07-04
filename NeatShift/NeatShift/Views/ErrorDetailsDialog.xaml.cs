using NeatShift.ViewModels;
using System.Windows;

namespace NeatShift.Views
{
    /// <summary>
    /// Interaction logic for ErrorDetailsDialog.xaml
    /// </summary>
    public partial class ErrorDetailsDialog : Window
    {
        public ErrorDetailsDialog(string errorMessage)
        {
            InitializeComponent();
            DataContext = new ErrorDetailsDialogViewModel(errorMessage);
        }
    }
} 