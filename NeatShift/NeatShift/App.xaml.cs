using Microsoft.Extensions.DependencyInjection;
using ModernWpf;
using NeatShift.Services;
using NeatShift.ViewModels;
using NeatShift.Views;
using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Windows;

namespace NeatShift
{
    [SupportedOSPlatform("windows7.0")]
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        [SupportedOSPlatform("windows7.0")]
        protected override void OnStartup(StartupEventArgs e)
        {
            // Check if running as administrator
            bool isAdmin = IsRunningAsAdministrator();
            if (!isAdmin)
            {
                // Restart the application with admin rights
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = Process.GetCurrentProcess().MainModule?.FileName,
                        Verb = "runas"
                    };

                    Process.Start(processInfo);
                    Current.Shutdown();
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to restart with administrator privileges: {ex.Message}", 
                        "Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                }
            }

            var services = new ServiceCollection();

            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = new MainWindow(_serviceProvider.GetRequiredService<MainWindowViewModel>());
            mainWindow.Show();
        }

        [SupportedOSPlatform("windows")]
        private static bool IsRunningAsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        [SupportedOSPlatform("windows7.0")]
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<ISystemRestoreService, SystemRestoreService>();
            services.AddSingleton<IFileOperationService, FileOperationService>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddHttpClient();
        }
    }
} 