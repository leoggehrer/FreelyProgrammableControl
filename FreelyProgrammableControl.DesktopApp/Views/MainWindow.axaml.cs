using Avalonia.Controls;
using Avalonia.Input;
using FreelyProgrammableControl.DesktopApp.ViewModels;
using System;

namespace FreelyProgrammableControl.DesktopApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.Initialize(StorageProvider, this);
            }
        }

        private void Output_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is Border border && border.DataContext is OutputDeviceViewModel outputViewModel)
            {
                outputViewModel.EditLabelCommand?.Execute(null);
            }
        }

        private void Input_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is Border border && border.DataContext is InputDeviceViewModel inputViewModel)
            {
                inputViewModel.EditLabelCommand?.Execute(null);
            }
        }
    }
}
