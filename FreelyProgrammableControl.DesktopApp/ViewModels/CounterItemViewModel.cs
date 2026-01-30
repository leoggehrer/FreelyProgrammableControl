using CommunityToolkit.Mvvm.ComponentModel;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// Represents a view model for a counter item in the application.
    /// </summary>
    public partial class CounterItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _index;

        [ObservableProperty]
        private int _value;
    }
}
