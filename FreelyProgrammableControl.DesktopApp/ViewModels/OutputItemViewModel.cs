using CommunityToolkit.Mvvm.ComponentModel;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// View model for a single output channel displayed in the I/O panel.
    /// Exposes the channel index, label, and current boolean value.
    /// Output values are read-only from the UI perspective —
    /// they are set exclusively by the running FPC program.
    /// </summary>
    public partial class OutputItemViewModel : ObservableObject
    {
        /// <summary>Zero-based index of this output channel.</summary>
        [ObservableProperty]
        private int _index;

        /// <summary>Display label for this output channel (e.g. "Output 0").</summary>
        [ObservableProperty]
        private string _label = string.Empty;

        /// <summary>Current boolean state of the output channel.</summary>
        [ObservableProperty]
        private bool _value;
    }
}
