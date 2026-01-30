using CommunityToolkit.Mvvm.ComponentModel;
using FreelyProgrammableControl.Logic.Input;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    public partial class InputDeviceViewModel : ViewModelBase
    {
        private readonly IInputDevice device;
        private bool isUpdating;

        public InputDeviceViewModel(IInputDevice device)
        {
            this.device = device;
            Label = device.Label;
            IsEnabled = device.Modifiable;
            UpdateFromDevice();
        }

        public string Label { get; }
        public bool IsEnabled { get; }

        [ObservableProperty]
        private bool isChecked;

        partial void OnIsCheckedChanged(bool value)
        {
            if (isUpdating)
            {
                return;
            }

            if (device is Switch sw && sw.Value != value)
            {
                sw.Toggle();
            }
        }

        public void UpdateFromDevice()
        {
            isUpdating = true;
            IsChecked = device.Value;
            isUpdating = false;
        }
    }
}
