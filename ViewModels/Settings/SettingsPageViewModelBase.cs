namespace WWXMapEditor.ViewModels.Settings
{
    public abstract class SettingsPageViewModelBase : ViewModelBase
    {
        public abstract void LoadSettings();
        public abstract void SaveSettings();
        public abstract void ResetToDefaults();
    }
}