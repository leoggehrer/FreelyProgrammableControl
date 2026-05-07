using FreelyProgrammableControl.McpServer.Services;

namespace FreelyProgrammableControl.McpServer;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Task runTask;
        var transport = ResolveTransport(args);

        if (transport == "stdio")
        {
            var builder = Host.CreateApplicationBuilder(args);

            _ = builder.Logging.ClearProviders();
            _ = builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly(typeof(Program).Assembly);

            var desktopBaseUrl = builder.Configuration["DesktopApp:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "Konfiguration 'DesktopApp:BaseUrl' fehlt in appsettings.json. " +
                    "Bitte z.B. \"DesktopApp\": { \"BaseUrl\": \"http://localhost:5555\" } eintragen.");

            DesktopClientProvider.Initialize(desktopBaseUrl);

            var app = builder.Build();
            runTask = app.RunAsync();
        }
        else
        {
            var webBuilder = WebApplication.CreateBuilder(args);

            _ = webBuilder.Services
                .AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly(typeof(Program).Assembly);

            var app = webBuilder.Build();
            var desktopBaseUrl = app.Configuration["DesktopApp:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "Konfiguration 'DesktopApp:BaseUrl' fehlt in appsettings.json. " +
                    "Bitte z.B. \"DesktopApp\": { \"BaseUrl\": \"http://localhost:5555\" } eintragen.");

            DesktopClientProvider.Initialize(desktopBaseUrl);

            app.MapMcp("/mcp");
            runTask = app.RunAsync();
        }
        await runTask;
    }

    private static string ResolveTransport(string[] args)
    {
        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];

            if (arg.StartsWith("--transport=", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeTransport(arg[("--transport=".Length)..]);
            }

            if (string.Equals(arg, "--transport", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                return NormalizeTransport(args[index + 1]);
            }

            if (string.Equals(arg, "http", StringComparison.OrdinalIgnoreCase) || string.Equals(arg, "stdio", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeTransport(arg);
            }
        }

        return "http";
    }

    private static string NormalizeTransport(string value)
    {
        if (string.Equals(value, "http", StringComparison.OrdinalIgnoreCase))
        {
            return "http";
        }

        if (string.Equals(value, "stdio", StringComparison.OrdinalIgnoreCase))
        {
            return "stdio";
        }

        throw new InvalidOperationException($"Unsupported transport '{value}'. Use 'http' or 'stdio'.");
    }

}
