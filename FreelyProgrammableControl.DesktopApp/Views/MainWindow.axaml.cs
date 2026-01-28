using Avalonia.Controls;
using FreelyProgrammableControl.DesktopApp.ViewModels;

namespace FreelyProgrammableControl.DesktopApp.Views
{
    /// <summary>
    /// Represents the main window of the application.
    /// </summary>
    /// <remarks>
    /// Der Code-Behind ist minimal und dient nur der Initialisierung.
    /// Die Geschäftslogik befindet sich im MainWindowViewModel.
    /// </remarks>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}
