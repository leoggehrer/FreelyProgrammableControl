using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// Represents a view model for an input item in the application.
    /// </summary>
    public partial class InputItemViewModel : ObservableObject
    {
        private Action<int, bool>? _onValueChanged;

        [ObservableProperty]
        private int _index;

        [ObservableProperty]
        private string _label = string.Empty;

        [ObservableProperty]
        private bool _value;

        [ObservableProperty]
        private bool _isModifiable;

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
