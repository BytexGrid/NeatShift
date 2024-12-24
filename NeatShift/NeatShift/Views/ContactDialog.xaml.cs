using System.Diagnostics;
using System.Windows;
using ModernWpf.Controls;

namespace NeatShift.Views
{
    public partial class ContactDialog : ContentDialog
    {
        // These will be replaced with actual links later
        private const string REDDIT_LINK = ""; // Temporarily removed as community is being set up
        private const string TELEGRAM_LINK = "https://t.me/NeatShift";
        private const string DISCORD_LINK = "https://discord.gg/tc3AjBRQq9";

        public ContactDialog()
        {
            InitializeComponent();
        }

        private void Reddit_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Reddit community is currently being set up. Please check back later.", 
                          "Coming Soon", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Information);
        }

        private void Telegram_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = TELEGRAM_LINK, UseShellExecute = true });
        }

        private void Discord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = DISCORD_LINK, UseShellExecute = true });
        }
    }
} 