using Avalonia;
using System;

namespace FreelyProgrammableControl.DesktopApp
{
    /// <summary>
    /// The entry point for the application. This class is responsible for initializing
    /// and starting the Avalonia application.
    /// </summary>
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        /// <summary>
        /// The entry point of the application. Initializes and starts the Avalonia application
        /// with a classic desktop lifetime.
        /// </summary>
        /// <param name="args">An array of command-line arguments passed to the application.</param>
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        /// <summary>
        /// Configures and builds an instance of the Avalonia application.
        /// </summary>
        /// <returns>An <see cref="AppBuilder"/> instance configured for the application.</returns>
        /// <remarks>
        /// This method sets up the application with platform detection,
        /// uses the Inter font, and enables logging to trace.
        /// </remarks>
        public static AppBuilder BuildAvaloniaApp()
                    => AppBuilder.Configure<App>()
                        .UsePlatformDetect()
                        .WithInterFont()
                        .LogToTrace();
    }
}
