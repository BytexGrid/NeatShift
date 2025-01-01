using ModernWpf.Controls;

namespace NeatShift.Views
{
    public partial class RestorePointDialog : ContentDialog
    {
        public RestorePointDialog()
        {
            InitializeComponent();
            DataContext = new ViewModels.RestorePointViewModel();
        }
    }
} 