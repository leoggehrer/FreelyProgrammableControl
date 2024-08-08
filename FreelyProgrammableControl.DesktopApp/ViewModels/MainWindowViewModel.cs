using Avalonia.Interactivity;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// Represents the view model for the main window of the application.
    /// </summary>
    /// <remarks>
    /// This class inherits from <see cref="ViewModelBase"/> and provides data and
    /// functionality for the main window's user interface.
    /// </remarks>
    public partial class MainWindowViewModel : ViewModelBase
    {
#pragma warning disable CA1822 // Mark members as static
        /// <summary>
        /// Gets a greeting message.
        /// </summary>
        /// <value>
        /// A string that contains the greeting message "Welcome to Avalonia!".
        /// </value>
        public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
    }
}
