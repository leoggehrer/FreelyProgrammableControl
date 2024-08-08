namespace FreelyProgrammableControl.ConApp
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <remarks>
    /// This class is responsible for initializing and running the FPC application.
    /// </remarks>
    internal class Program
    {
        /// <summary>
        /// The entry point of the application.
        /// </summary>
        /// <param name="args">An array of command-line arguments passed to the application.</param>
        static void Main(string[] args)
        {
            FPCApp app = new FPCApp();

            app.Run(args);
        }
    }
}
