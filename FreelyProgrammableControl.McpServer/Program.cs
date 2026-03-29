using FreelyProgrammableControl.McpServer.Tools;

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

        // Desktop-App URL aus Configuration laden
        var desktopBaseUrl = app.Configuration["DesktopApp:BaseUrl"];
        
        if (!string.IsNullOrWhiteSpace(desktopBaseUrl))
        {
            DesktopControlTool.Configure(desktopBaseUrl);
        }

        // MCP via MapMcp (Standard)
        app.MapMcp("/mcp");

        app.Run();
    }
}