using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreelyProgrammableControl.Logic.Output;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    public partial class OutputDeviceViewModel : ViewModelBase
    {
        private readonly IOutputDevice device;
        private readonly Window? ownerWindow;

        public OutputDeviceViewModel(IOutputDevice device, int index, Window? owner = null)
        {
            this.device = device;
            this.ownerWindow = owner;
            Label = device.Label;
            GroupName = index.ToString();
            UpdateFromDevice();
        }

        [ObservableProperty]
        private string label = string.Empty;
        
        public string GroupName { get; }

        [ObservableProperty]
        private bool isChecked;

        public void UpdateFromDevice()
        {
            IsChecked = device.Value;
        }

        [RelayCommand]
        private async Task EditLabel()
        {
            if (ownerWindow == null)
                return;

            var dialog = new Window
            {
                Title = "Label ändern",
                Width = 400,
                Height = 180,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var textBox = new TextBox
            {
                Text = Label,
                Watermark = "Neuer Label",
                Margin = new Avalonia.Thickness(10)
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 100,
                Margin = new Avalonia.Thickness(5)
            };

            var cancelButton = new Button
            {
                Content = "Abbrechen",
                Width = 100,
                Margin = new Avalonia.Thickness(5)
            };

            bool? result = null;

            okButton.Click += (s, e) => 
            {
                result = true;
                dialog.Close();
            };

            cancelButton.Click += (s, e) => 
            {
                result = false;
                dialog.Close();
            };

            textBox.KeyDown += (s, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Enter)
                {
                    result = true;
                    dialog.Close();
                }
                else if (e.Key == Avalonia.Input.Key.Escape)
                {
                    result = false;
                    dialog.Close();
                }
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Children = { okButton, cancelButton }
            };

            dialog.Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 15,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Geben Sie einen neuen Label ein:",
                        FontSize = 14
                    },
                    textBox,
                    buttonPanel
                }
            };

            await dialog.ShowDialog(ownerWindow);

            if (result == true && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                Label = textBox.Text;
                device.Label = textBox.Text;
            }
        }
    }
}
