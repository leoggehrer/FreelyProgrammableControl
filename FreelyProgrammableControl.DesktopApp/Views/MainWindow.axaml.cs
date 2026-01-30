using Avalonia.Controls;
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
    }
}
