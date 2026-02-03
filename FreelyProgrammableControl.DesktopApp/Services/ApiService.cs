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
    public class ApiService
    {
        private IWebHost? _webHost;
        private readonly MainWindowViewModel _viewModel;
        private readonly int _port;

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
                                isRunning = _viewModel.ExecutionUnit?.IsRunning ?? false,
                                hasParseError = _viewModel.ExecutionUnit?.HasParseError ?? false,
                                parseErrorMessage = _viewModel.ExecutionUnit?.ParseErrorMessage,
                                debugEnabled = _viewModel.ExecutionUnit?.DebugEnabled ?? false,
                                sourceLines = _viewModel.ExecutionUnit?.Source.Length ?? 0,
                                executionState = _viewModel.ExecutionState
                            };
                            await context.Response.WriteAsJsonAsync(response);
                        });

                        // Program laden
                        endpoints.MapPost("/api/program", async context =>
                        {
                            // Prüfe ob Steuerung läuft
                            if (_viewModel.ExecutionUnit?.IsRunning ?? false)
                            {
                                context.Response.StatusCode = 409; // Conflict
                                await context.Response.WriteAsJsonAsync(new { error = "Die Steuerung muss zuerst gestoppt werden, bevor ein neues Programm geladen werden kann" });
                                return;
                            }

                            using var reader = new StreamReader(context.Request.Body);
                            var programCode = await reader.ReadToEndAsync();

                            if (string.IsNullOrWhiteSpace(programCode))
                            {
                                context.Response.StatusCode = 400;
                                await context.Response.WriteAsJsonAsync(new { error = "Programm-Code darf nicht leer sein" });
                                return;
                            }

                            var lines = programCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                            
                            // Update ViewModel
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                _viewModel.SourceText = programCode;
                                _viewModel.ExecutionUnit?.LoadSource(lines);
                            });

                            var response = new
                            {
                                success = !(_viewModel.ExecutionUnit?.HasParseError ?? true),
                                hasParseError = _viewModel.ExecutionUnit?.HasParseError ?? false,
                                parseErrorMessage = _viewModel.ExecutionUnit?.ParseErrorMessage,
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

                        // Programm starten
                        endpoints.MapPost("/api/start", async context =>
                        {
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                _viewModel.ExecutionUnit?.Start();
                            });

                            var response = new
                            {
                                success = true,
                                isRunning = _viewModel.ExecutionUnit?.IsRunning ?? false
                            };
                            await context.Response.WriteAsJsonAsync(response);
                        });

                        // Programm stoppen
                        endpoints.MapPost("/api/stop", async context =>
                        {
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                _viewModel.ExecutionUnit?.Stop();
                            });

                            var response = new
                            {
                                success = true,
                                isRunning = _viewModel.ExecutionUnit?.IsRunning ?? false
                            };
                            await context.Response.WriteAsJsonAsync(response);
                        });

                        // Programm abrufen
                        endpoints.MapGet("/api/program", async context =>
                        {
                            var response = new
                            {
                                programCode = _viewModel.SourceText,
                                sourceLines = _viewModel.ExecutionUnit?.Source.Length ?? 0
                            };
                            await context.Response.WriteAsJsonAsync(response);
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
                            if (_viewModel.ExecutionUnit?.IsRunning ?? false)
                            {
                                context.Response.StatusCode = 409; // Conflict
                                await context.Response.WriteAsJsonAsync(new 
                                { 
                                    success = false,
                                    debugEnabled = _viewModel.ExecutionUnit?.DebugEnabled ?? false,
                                    error = "Debug-Modus kann nur geändert werden, wenn die Steuerung gestoppt ist" 
                                });
                                return;
                            }

                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                if (_viewModel.ExecutionUnit != null)
                                {
                                    _viewModel.ExecutionUnit.DebugEnabled = enable;
                                }
                            });

                            var response = new
                            {
                                success = true,
                                debugEnabled = _viewModel.ExecutionUnit?.DebugEnabled ?? false
                            };
                            await context.Response.WriteAsJsonAsync(response);
                        });

                        // Programmschritt ausführen
                        endpoints.MapPost("/api/step", async context =>
                        {
                            // Prüfe ob Steuerung läuft und Debug-Modus aktiviert ist
                            if (!(_viewModel.ExecutionUnit?.IsRunning ?? false))
                            {
                                context.Response.StatusCode = 409;
                                await context.Response.WriteAsJsonAsync(new 
                                { 
                                    success = false,
                                    error = "Die Steuerung muss gestartet sein" 
                                });
                                return;
                            }

                            if (!(_viewModel.ExecutionUnit?.DebugEnabled ?? false))
                            {
                                context.Response.StatusCode = 409;
                                await context.Response.WriteAsJsonAsync(new 
                                { 
                                    success = false,
                                    error = "Debug-Modus ist nicht aktiviert" 
                                });
                                return;
                            }

                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                _viewModel.ExecutionUnit?.Step();
                            });

                            // Warte kurz, damit der Step verarbeitet wird
                            await Task.Delay(50);

                            var response = new
                            {
                                success = true,
                                executionState = _viewModel.ExecutionUnit?.State
                            };
                            await context.Response.WriteAsJsonAsync(response);
                        });

                        // Ausführungszustand abrufen
                        endpoints.MapGet("/api/state", async context =>
                        {
                            var response = new
                            {
                                isRunning = _viewModel.ExecutionUnit?.IsRunning ?? false,
                                debugEnabled = _viewModel.ExecutionUnit?.DebugEnabled ?? false,
                                executionState = _viewModel.ExecutionUnit?.State
                            };
                            await context.Response.WriteAsJsonAsync(response);
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
    }
}
