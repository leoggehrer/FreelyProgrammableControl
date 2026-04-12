using CommunityToolkit.Mvvm.ComponentModel;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// View model for a single counter displayed in the debug panel.
    /// Exposes the counter index and its current integer value.
    /// Counter values are updated by the running FPC program via CINC/CDEC/CSET instructions.
    /// </summary>
    public partial class CounterItemViewModel : ObservableObject
    {
        /// <summary>Zero-based index of this counter (C0–C127).</summary>
        [ObservableProperty]
        private int _index;

        /// <summary>Current integer value of the counter.</summary>
        [ObservableProperty]
        private int _value;
    }
}
