using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using FreelyProgrammableControl.DesktopApp.ViewModels;
using FreelyProgrammableControl.Logic.Input;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

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
                                debugEnabled = _viewModel.DebugEnabled,
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
                                var (success, errorMessage) = _viewModel.StartForApi();

                                var response = new
                                {
                                    success,
                                    isRunning = _viewModel.IsRunning,
                                    error = errorMessage
                                };

                                if (!success)
                                {
                                    context.Response.StatusCode = 400;
                                }

                                await context.Response.WriteAsJsonAsync(response);
                            });
                            System.Diagnostics.Debug.WriteLine($"[API] Programm gestartet: {_viewModel.IsRunning}");
                        });

                        // Programm stoppen
                        endpoints.MapPost("/api/stop", async context =>
                        {
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                var (success, errorMessage) = _viewModel.StopForApi();

                                var response = new
                                {
                                    success,
                                    isRunning = _viewModel.IsRunning,
                                    error = errorMessage
                                };
                                await context.Response.WriteAsJsonAsync(response);
                            });
                            System.Diagnostics.Debug.WriteLine($"[API] Programm gestoppt: {!_viewModel.IsRunning}");
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
                                    debugEnabled = _viewModel.DebugEnabled
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

                        // ====================================================
                        // Debug & Inspection Endpoints
                        // ====================================================

                        // Vollständigen Debug-Snapshot abrufen (Stack, Memory, Timer, Counter, aktuelle Zeile)
                        endpoints.MapGet("/api/debug/snapshot", async context =>
                        {
                            var eu = _viewModel.GetExecutionUnit();
                            var response = new
                            {
                                isRunning = _viewModel.IsRunning,
                                debugEnabled = _viewModel.DebugEnabled,
                                state = eu.State,
                                debugInfo = eu.DebugInfo,
                                stackInfo = eu.StackInfo,
                                memoryInfo = eu.MemoryInfo,
                                countersInfo = eu.CountersInfo,
                                timersInfo = eu.TimersInfo,
                                inputsInfo = eu.InputsInfo,
                                outputsInfo = eu.OutputsInfo,
                                currentLine = eu.CurrentExecutionLine != null ? new
                                {
                                    lineNumber = eu.CurrentExecutionLine.LineNumber,
                                    source = eu.CurrentExecutionLine.Source,
                                    isComment = eu.CurrentExecutionLine.IsComment,
                                    hasError = eu.CurrentExecutionLine.HasError,
                                    errorMessage = eu.CurrentExecutionLine.ErrorMessage
                                } : null,
                                hasParseError = eu.HasParseError,
                                parseErrorMessage = eu.ParseErrorMessage,
                                hasExecutionError = eu.HasExecutionError,
                                executionErrorMessage = eu.ExecutionErrorMessage
                            };
                            await context.Response.WriteAsJsonAsync(response);
                            System.Diagnostics.Debug.WriteLine($"[API] Debug-Snapshot abgefragt");
                        });

                        // Input-Zustände abrufen
                        endpoints.MapGet("/api/inputs", async context =>
                        {
                            var eu = _viewModel.GetExecutionUnit();
                            var inputs = new List<object>();
                            for (int i = 0; i < eu.Inputs.Length; i++)
                            {
                                inputs.Add(new
                                {
                                    index = i,
                                    value = eu.Inputs.GetValue(i),
                                    label = eu.Inputs[i].Label
                                });
                            }
                            await context.Response.WriteAsJsonAsync(new { count = eu.Inputs.Length, inputs });
                            System.Diagnostics.Debug.WriteLine($"[API] Inputs abgefragt: {eu.Inputs.Length}");
                        });

                        // Einzelnen Input-Zustand abrufen
                        endpoints.MapGet("/api/inputs/{index}", async context =>
                        {
                            var eu = _viewModel.GetExecutionUnit();
                            if (!int.TryParse(context.Request.RouteValues["index"]?.ToString(), out var index)
                                || index < 0 || index >= eu.Inputs.Length)
                            {
                                context.Response.StatusCode = 400;
                                await context.Response.WriteAsJsonAsync(new { error = $"Ungültiger Index. Gültig: 0-{eu.Inputs.Length - 1}" });
                                return;
                            }
                            await context.Response.WriteAsJsonAsync(new
                            {
                                index,
                                value = eu.Inputs.GetValue(index),
                                label = eu.Inputs[index].Label
                            });
                        });

                        // Input umschalten (Toggle)
                        endpoints.MapPost("/api/inputs/{index}/toggle", async context =>
                        {
                            var eu = _viewModel.GetExecutionUnit();
                            if (!int.TryParse(context.Request.RouteValues["index"]?.ToString(), out var index)
                                || index < 0 || index >= eu.Inputs.Length)
                            {
                                context.Response.StatusCode = 400;
                                await context.Response.WriteAsJsonAsync(new { error = $"Ungültiger Index. Gültig: 0-{eu.Inputs.Length - 1}" });
                                return;
                            }

                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                if (eu.Inputs[index] is Switch sw && sw.Modifiable)
                                {
                                    sw.Toggle();
                                    await context.Response.WriteAsJsonAsync(new
                                    {
                                        success = true,
                                        index,
                                        value = eu.Inputs.GetValue(index),
                                        label = eu.Inputs[index].Label
                                    });
                                }
                                else
                                {
                                    context.Response.StatusCode = 400;
                                    await context.Response.WriteAsJsonAsync(new
                                    {
                                        success = false,
                                        error = $"Input {index} ist nicht umschaltbar (kein Switch oder nicht modifizierbar)"
                                    });
                                }
                            });
                            System.Diagnostics.Debug.WriteLine($"[API] Input {index} getoggelt");
                        });

                        // Output-Zustände abrufen
                        endpoints.MapGet("/api/outputs", async context =>
                        {
                            var eu = _viewModel.GetExecutionUnit();
                            var outputs = new List<object>();
                            for (int i = 0; i < eu.Outputs.Length; i++)
                            {
                                outputs.Add(new
                                {
                                    index = i,
                                    value = eu.Outputs.GetValue(i),
                                    label = eu.Outputs[i].Label
                                });
                            }
                            await context.Response.WriteAsJsonAsync(new { count = eu.Outputs.Length, outputs });
                            System.Diagnostics.Debug.WriteLine($"[API] Outputs abgefragt: {eu.Outputs.Length}");
                        });

                        // Memory-Werte abrufen (mit optionalem Bereich)
                        endpoints.MapGet("/api/memory", async context =>
                        {
                            var eu = _viewModel.GetExecutionUnit();
                            var from = 0;
                            var to = Math.Min(63, eu.MemoryLength - 1);

                            if (context.Request.Query.ContainsKey("from"))
                                int.TryParse(context.Request.Query["from"], out from);
                            if (context.Request.Query.ContainsKey("to"))
                                int.TryParse(context.Request.Query["to"], out to);

                            from = Math.Clamp(from, 0, eu.MemoryLength - 1);
                            to = Math.Clamp(to, from, eu.MemoryLength - 1);

                            var values = new List<object>();
                            for (int i = from; i <= to; i++)
                            {
                                values.Add(new { index = i, value = eu.GetMemoryValue(i) });
                            }
                            await context.Response.WriteAsJsonAsync(new
                            {
                                totalSize = eu.MemoryLength,
                                from,
                                to,
                                values
                            });
                            System.Diagnostics.Debug.WriteLine($"[API] Memory abgefragt: {from}-{to}");
                        });

                        // Timer-Zustände abrufen
                        endpoints.MapGet("/api/timers", async context =>
                        {
                            var eu = _viewModel.GetExecutionUnit();
                            var from = 0;
                            var to = Math.Min(15, eu.TimerLength - 1);

                            if (context.Request.Query.ContainsKey("from"))
                                int.TryParse(context.Request.Query["from"], out from);
                            if (context.Request.Query.ContainsKey("to"))
                                int.TryParse(context.Request.Query["to"], out to);

                            from = Math.Clamp(from, 0, eu.TimerLength - 1);
                            to = Math.Clamp(to, from, eu.TimerLength - 1);

                            var values = new List<object>();
                            for (int i = from; i <= to; i++)
                            {
                                values.Add(new { index = i, value = eu.GetTimerValue(i) });
                            }
                            await context.Response.WriteAsJsonAsync(new
                            {
                                totalCount = eu.TimerLength,
                                from,
                                to,
                                values
                            });
                            System.Diagnostics.Debug.WriteLine($"[API] Timers abgefragt: {from}-{to}");
                        });

                        // Counter-Werte abrufen
                        endpoints.MapGet("/api/counters", async context =>
                        {
                            var eu = _viewModel.GetExecutionUnit();
                            var from = 0;
                            var to = Math.Min(15, eu.Counters.Length - 1);

                            if (context.Request.Query.ContainsKey("from"))
                                int.TryParse(context.Request.Query["from"], out from);
                            if (context.Request.Query.ContainsKey("to"))
                                int.TryParse(context.Request.Query["to"], out to);

                            from = Math.Clamp(from, 0, eu.Counters.Length - 1);
                            to = Math.Clamp(to, from, eu.Counters.Length - 1);

                            var values = new List<object>();
                            for (int i = from; i <= to; i++)
                            {
                                values.Add(new { index = i, value = eu.Counters.GetValue(i) });
                            }
                            await context.Response.WriteAsJsonAsync(new
                            {
                                totalCount = eu.Counters.Length,
                                from,
                                to,
                                values
                            });
                            System.Diagnostics.Debug.WriteLine($"[API] Counters abgefragt: {from}-{to}");
                        });

                        // Zykluszeit abrufen/setzen
                        endpoints.MapGet("/api/cycletime", async context =>
                        {
                            var eu = _viewModel.GetExecutionUnit();
                            await context.Response.WriteAsJsonAsync(new
                            {
                                cycleTimeMs = eu.CycleTimeMs
                            });
                        });

                        endpoints.MapPost("/api/cycletime", async context =>
                        {
                            using var reader = new StreamReader(context.Request.Body);
                            var body = await reader.ReadToEndAsync();

                            if (!int.TryParse(body, out var cycleTimeMs) || cycleTimeMs < 1)
                            {
                                context.Response.StatusCode = 400;
                                await context.Response.WriteAsJsonAsync(new { error = "Ungültiger Wert. Erwartet: positive Ganzzahl (Millisekunden, min. 1)" });
                                return;
                            }

                            var eu = _viewModel.GetExecutionUnit();
                            eu.CycleTimeMs = cycleTimeMs;
                            await context.Response.WriteAsJsonAsync(new
                            {
                                success = true,
                                cycleTimeMs = eu.CycleTimeMs
                            });
                            System.Diagnostics.Debug.WriteLine($"[API] Zykluszeit gesetzt: {cycleTimeMs}ms");
                        });

                        // Outputs zurücksetzen
                        endpoints.MapPost("/api/reset/outputs", async context =>
                        {
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                var eu = _viewModel.GetExecutionUnit();
                                // Alle Outputs auf false setzen
                                for (int i = 0; i < eu.Outputs.Length; i++)
                                {
                                    eu.Outputs[i].Value = false;
                                }
                                await context.Response.WriteAsJsonAsync(new { success = true, message = "Alle Outputs zurückgesetzt" });
                            });
                            System.Diagnostics.Debug.WriteLine($"[API] Outputs zurückgesetzt");
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
