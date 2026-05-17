using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreelyProgrammableControl.Logic.Contracts;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// View model for a single output device in the I/O panel.
    /// Reflects the current output state set by the running FPC program
    /// and allows the user to rename the device label via a dialog.
    /// </summary>
    public partial class OutputDeviceViewModel : ViewModelBase
    {
        private readonly IOutputDevice device;
        private readonly Window? ownerWindow;
        private readonly Func<bool>? canEditLabel;

        /// <summary>
        /// Initialises the view model and reads the initial state from <paramref name="device"/>.
        /// </summary>
        /// <param name="device">The output device to represent.</param>
        /// <param name="index">Zero-based index used as the radio-button group name.</param>
        /// <param name="owner">Parent window used as dialog owner. May be null.</param>
        public OutputDeviceViewModel(IOutputDevice device, int index, Window? owner = null, Func<bool>? canEditLabel = null)
        {
            this.device = device;
            this.ownerWindow = owner;
            this.canEditLabel = canEditLabel;
            Label = device.Label;
            GroupName = index.ToString();
            UpdateFromDevice();
        }

        /// <summary>Display label of the output device.</summary>
        [ObservableProperty]
        private string label = string.Empty;

        /// <summary>
        /// Radio-button group name derived from the device index.
        /// Ensures each output indicator belongs to its own group.
        /// </summary>
        public string GroupName { get; }

        /// <summary>Current active state of the output device.</summary>
        [ObservableProperty]
        private bool isChecked;

        /// <summary>
        /// Refreshes <see cref="IsChecked"/> from the underlying device value.
        /// Call this after the FPC program has executed a cycle.
        /// </summary>
        public void UpdateFromDevice()
        {
            IsChecked = device.Value;
        }

        public void SetLabel(string label)
        {
            var normalized = string.IsNullOrWhiteSpace(label) ? Label : label.Trim();
            Label = normalized;
            device.Label = normalized;
        }

        /// <summary>
        /// Opens a dialog allowing the user to rename this output device.
        /// No-op if no owner window is available.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditLabel))]
        private async Task EditLabel()
        {
            if (!CanEditLabel() || ownerWindow == null)
                return;

            var newLabel = await ShowLabelEditDialogAsync(ownerWindow, Label);
            if (newLabel != null)
            {
                SetLabel(newLabel);
            }
        }

        private bool CanEditLabel() => canEditLabel?.Invoke() ?? true;

        public void NotifyCanExecuteChanged()
        {
            EditLabelCommand.NotifyCanExecuteChanged();
        }
    }
}
