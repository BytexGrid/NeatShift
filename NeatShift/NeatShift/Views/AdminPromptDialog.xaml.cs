using ModernWpf.Controls;

namespace NeatShift.Views
{
    public partial class AdminPromptDialog : ContentDialog
    {
        public AdminPromptDialog(string reason, string actions)
        {
            InitializeComponent();
            ReasonText.Text = reason;
            ActionText.Text = actions;
        }
    }
} 