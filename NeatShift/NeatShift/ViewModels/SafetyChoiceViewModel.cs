using CommunityToolkit.Mvvm.ComponentModel;

namespace NeatShift.ViewModels
{
    public partial class SafetyChoiceViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _useNeatSavesOnly;

        [ObservableProperty]
        private bool _useSystemRestoreOnly;

        [ObservableProperty]
        private bool _useBoth = true; // Default choice

        [ObservableProperty]
        private bool _rememberChoice;

        partial void OnUseNeatSavesOnlyChanged(bool value)
        {
            if (value)
            {
                UseSystemRestoreOnly = false;
                UseBoth = false;
            }
        }

        partial void OnUseSystemRestoreOnlyChanged(bool value)
        {
            if (value)
            {
                UseNeatSavesOnly = false;
                UseBoth = false;
            }
        }

        partial void OnUseBothChanged(bool value)
        {
            if (value)
            {
                UseNeatSavesOnly = false;
                UseSystemRestoreOnly = false;
            }
        }
    }
} 