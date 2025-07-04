using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Windows;

namespace NeatShift.ViewModels
{
    public partial class ErrorDetailsDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _bugDescription;

        [ObservableProperty]
        private string _systemInfo;

        [ObservableProperty]
        private string _additionalContext;

        [ObservableProperty]
        private string _issueTitle;

        public ErrorDetailsDialogViewModel(string errorMessage)
        {
            _issueTitle = $"Bug: {errorMessage.Split('\n')[0]}";
            _bugDescription = errorMessage;
            _systemInfo = $"OS: {Environment.OSVersion}\n" +
                          $"App Version: {Version.Current}";
            _additionalContext = "See attached logs for details.";
        }

        [RelayCommand]
        private void OpenLogsFolder()
        {
            string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NeatShift", "logs");
            if (Directory.Exists(logDir))
            {
                Process.Start("explorer.exe", logDir);
            }
            else
            {
                MessageBox.Show("No log files have been generated yet.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string GetFormattedIssueBody()
        {
            var sb = new StringBuilder();
            sb.AppendLine("### Describe the bug");
            sb.AppendLine(BugDescription);
            sb.AppendLine("\n### To Reproduce");
            sb.AppendLine("Steps to reproduce the behavior:");
            sb.AppendLine("1. Go to '...'");
            sb.AppendLine("2. Click on '....'");
            sb.AppendLine("3. Scroll down to '....'");
            sb.AppendLine("4. See error");
            sb.AppendLine("\n### Expected behavior");
            sb.AppendLine("A clear and concise description of what you expected to happen.");
            sb.AppendLine("\n### System Information");
            sb.AppendLine("```");
            sb.AppendLine(SystemInfo);
            sb.AppendLine("```");
            sb.AppendLine("\n### Additional context");
            sb.AppendLine(AdditionalContext);
            return sb.ToString();
        }

        [RelayCommand]
        private void CopyToClipboard()
        {
            Clipboard.SetText(GetFormattedIssueBody());
            MessageBox.Show("Issue details copied to clipboard.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void SubmitOnGitHub()
        {
            string baseUrl = "https://github.com/BytexGrid/NeatShift/issues/new";
            string title = Uri.EscapeDataString(IssueTitle);
            string body = Uri.EscapeDataString(GetFormattedIssueBody());
            string url = $"{baseUrl}?title={title}&body={body}";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
} 