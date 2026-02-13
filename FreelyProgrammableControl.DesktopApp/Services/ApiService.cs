using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using System.IO;
using FreelyProgrammableControl.DesktopApp.ViewModels;
using Microsoft.AspNetCore.Http;

namespace FreelyProgrammableControl.DesktopApp.Services
{
    /// <summary>
    /// HTTP API Service für die Remote-Steuerung der Desktop-Anwendung
    /// </summary>
    public class ApiService : IDisposable
    {
        private IWebHost? _webHost;
        private readonly MainWindowViewModel _viewModel;
        private readonly int _port;
        private bool _disposed;

        public bool IsRunning => _webHost != null;
        public int Port => _port;

        public ApiService(MainWindowViewModel viewModel, int port = 5555)
        {
            _viewModel = viewModel;
            _port = port;
        }

        /// <summary>
        /// Startet den HTTP-Server
        /// </summary>
        public Task StartAsync()
        {
            if (_webHost != null)
                return Task.CompletedTask;

            _webHost = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://localhost:{_port}")
                .ConfigureServices(services =>
                {
                    services.AddSingleton(_viewModel);
                    services.AddRouting();
                    services.AddCors(options =>
                    {
                        options.AddDefaultPolicy(policy =>
                        {
                            policy.AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  .AllowAnyHeader();
                        });
                    });
                })
                .Configure(app =>
                {
                    app.UseCors();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        // Status-Endpunkt
                        endpoints.MapGet("/api/status", async context =>
                        {
                            var response = new
                            {
                                isRunning = _viewModel.IsRunning,
                                hasParseError = _viewModel.HasParseError,
                                parseErrorMessage = _viewModel.ParseErrorMessage,
                                debugEnabled = _viewModel.IsDebugEnabled,
                                sourceLines = _viewModel.Source.Length,
                                executionState = _viewModel.ExecutionState
                            };
                            await context.Response.WriteAsJsonAsync(response);
                            System.Diagnostics.Debug.WriteLine($"[API] Status abgefragt: {response}");
                        });

                        // Program laden
                        endpoints.MapPost("/api/program", async context =>
                        {
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                _viewModel.StopCommand?.Execute(null);

                                using var reader = new StreamReader(context.Request.Body);
                                var programCode = await reader.ReadToEndAsync();

                                if (string.IsNullOrWhiteSpace(programCode))
                                {
                                    context.Response.StatusCode = 400;
                                    await context.Response.WriteAsJsonAsync(new { error = "Programm-Code darf nicht leer sein" });
                                    return;
                                }

                                var lines = programCode.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

                                _viewModel.SourceText = programCode;
                                _viewModel.LoadSourceCommand?.Execute(null);

                                var response = new
                                {
                                    success = !_viewModel.HasParseError,
                                    hasParseError = _viewModel.HasParseError,
                                    parseErrorMessage = _viewModel.ParseErrorMessage,
                                    sourceLines = lines.Length
                                };

                                if (response.success)
                                {
                                    await context.Response.WriteAsJsonAsync(response);
                                }
                                else
                                {
                                    context.Response.StatusCode = 400;
                                    await context.Response.WriteAsJsonAsync(response);
                                }
                            });
                            System.Diagnostics.Debug.WriteLine($"[API] Programm geladen: {context.Request.ContentLength} Bytes");
                        });

                        // Programm starten
                        endpoints.MapPost("/api/start", async context =>
                        {
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                _viewModel.StartCommand?.Execute(null);

                                var response = new
                                {
                                    success = true,
                                    isRunning = _viewModel.IsRunning
                                };
                                await context.Response.WriteAsJsonAsync(response);
                            });
                            System.Diagnostics.Debug.WriteLine($"[API] Programm gestartet");
                        });

                        // Programm stoppen
                        endpoints.MapPost("/api/stop", async context =>
                        {
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                _viewModel.StopCommand?.Execute(null);

                                var response = new
                                {
                                    success = true,
                                    isRunning = _viewModel.IsRunning
                                };
                                await context.Response.WriteAsJsonAsync(response);
                            });
                            System.Diagnostics.Debug.WriteLine($"[API] Programm gestoppt");
                        });

                        // Programm abrufen
                        endpoints.MapGet("/api/program", async context =>
                        {
                            var response = new
                            {
                                programCode = _viewModel.SourceText,
                                sourceLines = _viewModel.Source.Length
                            };
                            await context.Response.WriteAsJsonAsync(response);
                            System.Diagnostics.Debug.WriteLine($"[API] Programm abgerufen: {response.sourceLines} Zeilen");
                        });

                        // Debug-Modus setzen
                        endpoints.MapPost("/api/debug", async context =>
                        {
                            using var reader = new StreamReader(context.Request.Body);
                            var enableText = await reader.ReadToEndAsync();

                            if (!bool.TryParse(enableText, out var enable))
                            {
                                context.Response.StatusCode = 400;
                                await context.Response.WriteAsJsonAsync(new { error = "Ungültiger Wert. Erwartet: true oder false" });
                                return;
                            }

                            // Prüfe ob Steuerung läuft
                            if (_viewModel.IsRunning)
                            {
                                context.Response.StatusCode = 409; // Conflict
                                await context.Response.WriteAsJsonAsync(new
                                {
                                    success = false,
                                    debugEnabled = _viewModel.DebugEnabled,
                                    error = "Debug-Modus kann nur geändert werden, wenn die Steuerung gestoppt ist"
                                });
                                return;
                            }

                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                _viewModel.DebugEnabled = enable;

                                var response = new
                                {
                                    success = true,
                                    debugEnabled = _viewModel.IsDebugEnabled
                                };
                                await context.Response.WriteAsJsonAsync(response);
                            });
                            System.Diagnostics.Debug.WriteLine($"[API] Debug-Modus gesetzt: {enable}");
                        });

                        // Programmschritt ausführen
                        endpoints.MapPost("/api/step", async context =>
                        {
                            // Prüfe ob Steuerung läuft und Debug-Modus aktiviert ist
                            if (!_viewModel.IsRunning)
                            {
                                context.Response.StatusCode = 409;
                                await context.Response.WriteAsJsonAsync(new
                                {
                                    success = false,
                                    error = "Die Steuerung muss gestartet sein"
                                });
                                return;
                            }

                            if (!_viewModel.DebugEnabled)
                            {
                                context.Response.StatusCode = 409;
                                await context.Response.WriteAsJsonAsync(new
                                {
                                    success = false,
                                    error = "Debug-Modus ist nicht aktiviert"
                                });
                                return;
                            }

                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                _viewModel.StepCommand?.Execute(null);

                                // Warte kurz, damit der Step verarbeitet wird
                                await Task.Delay(50);

                                var response = new
                                {
                                    success = true,
                                    executionState = _viewModel.State,
                                };
                                await context.Response.WriteAsJsonAsync(response);
                            });
                            System.Diagnostics.Debug.WriteLine($"[API] Programmschritt ausgeführt");
                        });

                        // Ausführungszustand abrufen
                        endpoints.MapGet("/api/state", async context =>
                        {
                            var response = new
                            {
                                isRunning = _viewModel.IsRunning,
                                debugEnabled = _viewModel.DebugEnabled,
                                executionState = _viewModel.State
                            };
                            await context.Response.WriteAsJsonAsync(response);
                            System.Diagnostics.Debug.WriteLine($"[API] Ausführungszustand abgefragt: {response}");
                        });
                    });
                })
                .Build();

            return _webHost.StartAsync();
        }

        /// <summary>
        /// Stoppt den HTTP-Server
        /// </summary>
        public async Task StopAsync()
        {
            if (_webHost != null)
            {
                await _webHost.StopAsync();
                _webHost.Dispose();
                _webHost = null;
            }
        }

        /// <summary>
        /// Dispose implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _webHost?.StopAsync().GetAwaiter().GetResult();
                    _webHost?.Dispose();
                    _webHost = null;
                }
                _disposed = true;
            }
        }
    }
}
