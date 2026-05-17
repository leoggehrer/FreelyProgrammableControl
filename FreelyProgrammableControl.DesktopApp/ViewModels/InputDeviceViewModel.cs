using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreelyProgrammableControl.Logic.Contracts;
using FreelyProgrammableControl.Logic.Input;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// View model for a single input device in the I/O panel.
    /// Synchronises the UI checkbox with the underlying <see cref="IInputDevice"/>
    /// and allows the user to rename the device label via a dialog.
    /// </summary>
    public partial class InputDeviceViewModel : ViewModelBase
    {
        private readonly IInputDevice device;
        private readonly Window? ownerWindow;
        private readonly Func<bool>? canEditLabel;
        private bool isUpdating;

        /// <summary>
        /// Initialises the view model and reads the initial state from <paramref name="device"/>.
        /// </summary>
        /// <param name="device">The input device to represent.</param>
        /// <param name="owner">Parent window used as dialog owner. May be null.</param>
        public InputDeviceViewModel(IInputDevice device, Window? owner = null, Func<bool>? canEditLabel = null)
        {
            this.device = device;
            this.ownerWindow = owner;
            this.canEditLabel = canEditLabel;
            Label = device.Label;
            IsEnabled = device.Modifiable;
            UpdateFromDevice();
        }

        /// <summary>Display label of the input device.</summary>
        [ObservableProperty]
        private string label = string.Empty;

        /// <summary>
        /// Whether the input can be toggled interactively.
        /// False for non-modifiable devices such as <see cref="Blinker"/>.
        /// </summary>
        public bool IsEnabled { get; }

        /// <summary>Current checked (active) state of the input device.</summary>
        [ObservableProperty]
        private bool isChecked;

        partial void OnIsCheckedChanged(bool value)
        {
            if (isUpdating)
                return;

            if (device is Switch sw && sw.Value != value)
                sw.Toggle();
        }

        /// <summary>
        /// Refreshes <see cref="IsChecked"/> from the underlying device value.
        /// Call this after the FPC program has executed a cycle.
        /// </summary>
        public void UpdateFromDevice()
        {
            isUpdating = true;
            IsChecked = device.Value;
            isUpdating = false;
        }

        public void SetLabel(string label)
        {
            var normalized = string.IsNullOrWhiteSpace(label) ? Label : label.Trim();
            Label = normalized;
            device.Label = normalized;
        }

        /// <summary>
        /// Opens a dialog allowing the user to rename this input device.
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
