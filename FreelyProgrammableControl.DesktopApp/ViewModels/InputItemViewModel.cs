using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// View model for a single input channel displayed in the I/O panel.
    /// Exposes the channel index, label, current boolean value, and whether
    /// the value can be changed interactively by the user.
    /// </summary>
    public partial class InputItemViewModel : ObservableObject
    {
        private Action<int, bool>? _onValueChanged;

        /// <summary>Zero-based index of this input channel.</summary>
        [ObservableProperty]
        private int _index;

        /// <summary>Display label for this input channel (e.g. "Switch 0").</summary>
        [ObservableProperty]
        private string _label = string.Empty;

        /// <summary>Current boolean state of the input channel.</summary>
        [ObservableProperty]
        private bool _value;

        /// <summary>
        /// Whether the user can toggle this input interactively.
        /// False for read-only inputs such as <see cref="FreelyProgrammableControl.Logic.Input.Blinker"/>.
        /// </summary>
        [ObservableProperty]
        private bool _isModifiable;

        /// <summary>
        /// Registers a callback that is invoked whenever <see cref="Value"/> changes.
        /// </summary>
        /// <param name="callback">Action receiving the channel index and the new value.</param>
        public void SetValueChangedCallback(Action<int, bool> callback)
        {
            _onValueChanged = callback;
        }

        partial void OnValueChanged(bool oldValue, bool newValue)
        {
            _onValueChanged?.Invoke(Index, newValue);
        }
    }
}
