using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using FreelyProgrammableControl.DesktopApp.ViewModels;
using FreelyProgrammableControl.DesktopApp.Views;

namespace FreelyProgrammableControl.DesktopApp
{
    /// <summary>
    /// Represents the main application class for the Avalonia application.
    /// </summary>
    /// <remarks>
    /// This class is responsible for initializing the application and setting up the main window.
    /// It derives from the <see cref="Application"/> class and overrides necessary methods
    /// for application lifecycle management.
    /// </remarks>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the current instance by loading the associated XAML.
        /// </summary>
        /// <remarks>
        /// This method overrides the base class implementation to ensure that
        /// the XAML for this component is loaded correctly at initialization.
        /// </remarks>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Called when the framework initialization is completed.
        /// </summary>
        /// <remarks>
        /// This method sets up the main window of the application
        /// and removes duplicate data validation from Avalonia and
        /// CommunityToolkit.Mvvm. If the application is running in a
        /// classic desktop style lifetime, it initializes the main window
        /// with a new instance of <see cref="MainWindow"/> and sets its
        /// <see cref="DataContext"/> to a new instance of
        /// <see cref="MainWindowViewModel"/>.
        /// </remarks>
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}