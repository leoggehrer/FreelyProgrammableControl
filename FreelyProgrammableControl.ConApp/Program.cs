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
            var (programPath, cycleTime) = ParseArgs(args);
            var app = new FPCApp(programPath, cycleTime);

            app.Run(args);
        }

        private static (string? programPath, int? cycleTimeMs) ParseArgs(string[] args)
        {
            string? programPath = null;
            int? cycleTimeMs = null;

            foreach (var arg in args)
            {
                if (arg.StartsWith("--program=", StringComparison.OrdinalIgnoreCase))
                {
                    programPath = arg["--program=".Length..];
                }
                else if (arg.StartsWith("--cycle=", StringComparison.OrdinalIgnoreCase)
                         && int.TryParse(arg["--cycle=".Length..], out var parsed))
                {
                    cycleTimeMs = parsed;
                }
            }

            return (programPath, cycleTimeMs);
        }
    }
}
