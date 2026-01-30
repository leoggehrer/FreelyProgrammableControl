using CommunityToolkit.Mvvm.ComponentModel;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// Represents a view model for an output item in the application.
    /// </summary>
    public partial class OutputItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _index;

        [ObservableProperty]
        private string _label = string.Empty;

        [ObservableProperty]
        private bool _value;
    }
}
