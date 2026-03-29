using FreelyProgrammableControl.McpServer.Services;

namespace FreelyProgrammableControl.McpServer;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        _ = builder.Services
            .AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly();

        var app = builder.Build();

        // Desktop-App URL einmalig aus appsettings.json laden
        var desktopBaseUrl = app.Configuration["DesktopApp:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Konfiguration 'DesktopApp:BaseUrl' fehlt in appsettings.json. " +
                "Bitte z.B. \"DesktopApp\": { \"BaseUrl\": \"http://localhost:5555\" } eintragen.");

        DesktopClientProvider.Initialize(desktopBaseUrl);

        // MCP via MapMcp (Standard)
        app.MapMcp("/mcp");

        app.Run();
    }
}
