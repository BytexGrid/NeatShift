using System;
using System.Diagnostics;
using ModernWpf.Controls;

namespace NeatShift.Views
{
    public partial class FeatureRequestDialog : ContentDialog
    {
        public FeatureRequestDialog()
        {
            InitializeComponent();
            this.PrimaryButtonClick += FeatureRequestDialog_PrimaryButtonClick;
        }

        private void FeatureRequestDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Get a deferral because we're going to do an async operation
            var deferral = args.GetDeferral();

            try
            {
                // Create GitHub issue URL with pre-filled content
                var title = Uri.EscapeDataString(TitleTextBox.Text);
                var description = Uri.EscapeDataString(
                    $"{DescriptionTextBox.Text}\n\n" +
                    $"**Use Case:**\n{UseCaseTextBox.Text}");

                var url = $"https://github.com/BytexGrid/NeatShift/issues/new?title={title}&body={description}&labels=enhancement";

                // Open in browser
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
} 