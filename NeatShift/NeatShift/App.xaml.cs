using Microsoft.Extensions.DependencyInjection;
using ModernWpf;
using NeatShift.Services;
using NeatShift.ViewModels;
using NeatShift.Views;
using NeatShift.Models;
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
        public IServiceProvider Services => _serviceProvider ?? throw new InvalidOperationException("Services not initialized");

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
            services.AddSingleton<ISystemRestoreService, SystemRestoreService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<INeatSavesService>(provider =>
            {
                var settingsService = provider.GetRequiredService<ISettingsService>();
                var settings = new NeatSavesSettings
                {
                    UseNeatSaves = settingsService.GetUseNeatSaves(),
                    MaxOperationsToKeep = settingsService.GetMaxNeatSaves(),
                    SaveLocation = settingsService.GetNeatSavesLocation()
                };
                return new NeatSavesService(settings);
            });
            services.AddSingleton<IFileOperationService, FileOperationService>();
            services.AddSingleton<IRecentLocationsService, RecentLocationsService>();
            services.AddSingleton<FileBrowserViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>(sp => new MainWindow(sp.GetRequiredService<MainWindowViewModel>()));
            services.AddHttpClient();
        }
    }
} 