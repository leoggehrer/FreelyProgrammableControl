using CommunityToolkit.Mvvm.ComponentModel;
using FreelyProgrammableControl.Logic.Output;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    public partial class OutputDeviceViewModel : ViewModelBase
    {
        private readonly IOutputDevice device;

        public OutputDeviceViewModel(IOutputDevice device, int index)
        {
            this.device = device;
            Label = device.Label;
            GroupName = index.ToString();
            UpdateFromDevice();
        }

        public string Label { get; }
        public string GroupName { get; }

        [ObservableProperty]
        private bool isChecked;

        public void UpdateFromDevice()
        {
            IsChecked = device.Value;
        }
    }
}
