using NeatShift.Models;

namespace NeatShift.Services
{
    public interface ISettingsService
    {
        Settings LoadSettings();
        void SaveSettings(Settings settings);
        bool GetCreateRestorePoint();
        void SetCreateRestorePoint(bool value);
        bool GetHideSymbolicLinks();
        void SetHideSymbolicLinks(bool value);
    }
} 