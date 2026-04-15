using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FreelyProgrammableControl.DesktopApp.ViewModels;
using FreelyProgrammableControl.Logic.Input;

namespace FreelyProgrammableControl.DesktopApp.Services
{
    /// <summary>
    /// Minimal HTTP API server that exposes the FPC Desktop Application state
    /// to external clients (primarily the MCP Server).
    /// Listens on <c>http://localhost:5555</c> by default.
    /// Each endpoint is implemented as a dedicated private handler method.
    /// </summary>
    public class ApiService : IDisposable
    {
        private IWebHost? _webHost;
        private readonly MainWindowViewModel _viewModel;
        private readonly int _port;
        private bool _disposed;

        /// <summary>Whether the Kestrel web host is currently running.</summary>
        public bool IsRunning => _webHost != null;

        /// <summary>The TCP port the server listens on.</summary>
        public int Port => _port;

        /// <summary>
        /// Creates a new <see cref="ApiService"/> bound to the given view model and port.
        /// Call <see cref="StartAsync"/> to begin accepting requests.
        /// </summary>
        public ApiService(MainWindowViewModel viewModel, int port = 5555)
        {
            _viewModel = viewModel;
            _port = port;
        }

        // ----------------------------------------------------------------
        // Lifecycle
        // ----------------------------------------------------------------

        /// <summary>Starts the Kestrel web host. No-op if already running.</summary>
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
                        options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
                })
                .Configure(app =>
                {
                    app.UseCors();
                    app.UseRouting();
                    app.UseEndpoints(RegisterEndpoints);
                })
                .Build();

            return _webHost.StartAsync();
        }

        /// <summary>Stops and disposes the Kestrel web host.</summary>
        public async Task StopAsync()
        {
            if (_webHost != null)
            {
                await _webHost.StopAsync();
                _webHost.Dispose();
                _webHost = null;
            }
        }

        // ----------------------------------------------------------------
        // Endpoint registration
        // ----------------------------------------------------------------

        private void RegisterEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/status",               HandleGetStatusAsync);
            endpoints.MapGet("/api/program",              HandleGetProgramAsync);
            endpoints.MapPost("/api/program",             HandlePostProgramAsync);
            endpoints.MapPost("/api/program/clear",       HandleClearProgramAsync);
            endpoints.MapPost("/api/start",               HandleStartAsync);
            endpoints.MapPost("/api/stop",                HandleStopAsync);
            endpoints.MapPost("/api/debug",               HandleSetDebugModeAsync);
            endpoints.MapPost("/api/step",                HandleStepAsync);
            endpoints.MapGet("/api/state",                HandleGetStateAsync);
            endpoints.MapGet("/api/debug/snapshot",       HandleGetDebugSnapshotAsync);
            endpoints.MapGet("/api/inputs",               HandleGetInputsAsync);
            endpoints.MapGet("/api/inputs/{index}",       HandleGetInputByIndexAsync);
            endpoints.MapPost("/api/inputs/{index}/toggle", HandleToggleInputAsync);
            endpoints.MapPut("/api/inputs/{index}/label",   HandleSetInputLabelAsync);
            endpoints.MapGet("/api/outputs",              HandleGetOutputsAsync);
            endpoints.MapPut("/api/outputs/{index}/label",  HandleSetOutputLabelAsync);
            endpoints.MapGet("/api/memory",               HandleGetMemoryAsync);
            endpoints.MapGet("/api/timers",               HandleGetTimersAsync);
            endpoints.MapGet("/api/counters",             HandleGetCountersAsync);
            endpoints.MapGet("/api/cycletime",            HandleGetCycleTimeAsync);
            endpoints.MapPost("/api/cycletime",           HandleSetCycleTimeAsync);
            endpoints.MapPost("/api/reset/outputs",       HandleResetOutputsAsync);
        }

        // ----------------------------------------------------------------
        // Execution control handlers
        // ----------------------------------------------------------------

        /// <summary>GET /api/status — returns current running/debug/parse state.</summary>
        private async Task HandleGetStatusAsync(HttpContext ctx)
        {
            await ctx.Response.WriteAsJsonAsync(new
            {
                isRunning        = _viewModel.IsRunning,
                hasParseError    = _viewModel.HasParseError,
                parseErrorMessage = _viewModel.ParseErrorMessage,
                debugEnabled     = _viewModel.DebugEnabled,
                sourceLines      = _viewModel.Source.Length,
                executionState   = _viewModel.ExecutionState
            });
            System.Diagnostics.Debug.WriteLine("[API] GET /api/status");
        }

        /// <summary>GET /api/program — returns the currently loaded source code.</summary>
        private async Task HandleGetProgramAsync(HttpContext ctx)
        {
            var sourceText = _viewModel.SourceText;
            if (string.IsNullOrWhiteSpace(sourceText) && _viewModel.Source.Length > 0)
            {
                sourceText = string.Join(Environment.NewLine, _viewModel.Source);
            }

            await ctx.Response.WriteAsJsonAsync(new
            {
                programCode = sourceText,
                sourceLines = _viewModel.Source.Length
            });
        }

        /// <summary>POST /api/program — loads new source code; stops the controller first.</summary>
        private async Task HandlePostProgramAsync(HttpContext ctx)
        {
            using var reader = new StreamReader(ctx.Request.Body);
            var programCode = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(programCode))
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.WriteAsJsonAsync(new { error = "Programm-Code darf nicht leer sein" });
                return;
            }

            var lines = programCode.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
            List<string> parseErrors = [];

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                _viewModel.StopCommand?.Execute(null);
                _viewModel.SourceText = programCode;
                _viewModel.LoadSourceCommand?.Execute(null);

                var eu = _viewModel.GetExecutionUnit();
                var parsedLines = eu.Parse(lines);
                parseErrors = parsedLines
                    .Where(pl => pl.HasError)
                    .Select(pl => $"Zeile {pl.LineNumber}: {pl.ErrorMessage}")
                    .ToList();
            });

            var response = new
            {
                success = parseErrors.Count == 0,
                hasParseError = parseErrors.Count > 0,
                parseErrorMessage = parseErrors.FirstOrDefault(),
                parseErrors = parseErrors,
                sourceLines = lines.Length
            };

            if (!response.success) ctx.Response.StatusCode = 400;
            await ctx.Response.WriteAsJsonAsync(response);
        }

        /// <summary>POST /api/program/clear — clears editor source and unloaded program state.</summary>
        private async Task HandleClearProgramAsync(HttpContext ctx)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (_viewModel.IsRunning)
                {
                    _viewModel.StopForApi();
                }

                _viewModel.SourceText = string.Empty;
                _viewModel.GetExecutionUnit().LoadSource(Array.Empty<string>());

                await ctx.Response.WriteAsJsonAsync(new
                {
                    success = true,
                    sourceLines = 0,
                    hasParseError = false,
                    parseErrorMessage = (string?)null
                });
            });

            System.Diagnostics.Debug.WriteLine("[API] POST /api/program/clear");
        }

        /// <summary>POST /api/start — starts program execution.</summary>
        private async Task HandleStartAsync(HttpContext ctx)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var (success, error) = _viewModel.StartForApi();
                if (!success) ctx.Response.StatusCode = 400;
                await ctx.Response.WriteAsJsonAsync(new { success, isRunning = _viewModel.IsRunning, error });
            });
            System.Diagnostics.Debug.WriteLine($"[API] POST /api/start → isRunning={_viewModel.IsRunning}");
        }

        /// <summary>POST /api/stop — stops program execution.</summary>
        private async Task HandleStopAsync(HttpContext ctx)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var (success, error) = _viewModel.StopForApi();
                await ctx.Response.WriteAsJsonAsync(new { success, isRunning = _viewModel.IsRunning, error });
            });
            System.Diagnostics.Debug.WriteLine($"[API] POST /api/stop → isRunning={_viewModel.IsRunning}");
        }

        // ----------------------------------------------------------------
        // Debug handlers
        // ----------------------------------------------------------------

        /// <summary>POST /api/debug — enables or disables debug mode (controller must be stopped).</summary>
        private async Task HandleSetDebugModeAsync(HttpContext ctx)
        {
            using var reader = new StreamReader(ctx.Request.Body);
            var body = await reader.ReadToEndAsync();

            if (!bool.TryParse(body, out var enable))
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.WriteAsJsonAsync(new { error = "Ungültiger Wert. Erwartet: true oder false" });
                return;
            }

            if (_viewModel.IsRunning)
            {
                ctx.Response.StatusCode = 409;
                await ctx.Response.WriteAsJsonAsync(new
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
                await ctx.Response.WriteAsJsonAsync(new { success = true, debugEnabled = _viewModel.DebugEnabled });
            });
            System.Diagnostics.Debug.WriteLine($"[API] POST /api/debug → enable={enable}");
        }

        /// <summary>POST /api/step — executes one instruction step in debug mode.</summary>
        private async Task HandleStepAsync(HttpContext ctx)
        {
            if (!_viewModel.IsRunning)
            {
                ctx.Response.StatusCode = 409;
                await ctx.Response.WriteAsJsonAsync(new { success = false, error = "Die Steuerung muss gestartet sein" });
                return;
            }

            if (!_viewModel.DebugEnabled)
            {
                ctx.Response.StatusCode = 409;
                await ctx.Response.WriteAsJsonAsync(new { success = false, error = "Debug-Modus ist nicht aktiviert" });
                return;
            }

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                _viewModel.StepCommand?.Execute(null);
                await Task.Delay(50); // allow the step to be processed
                await ctx.Response.WriteAsJsonAsync(new { success = true, executionState = _viewModel.State });
            });
            System.Diagnostics.Debug.WriteLine("[API] POST /api/step");
        }

        /// <summary>GET /api/state — returns running state, debug flag and execution state text.</summary>
        private async Task HandleGetStateAsync(HttpContext ctx)
        {
            await ctx.Response.WriteAsJsonAsync(new
            {
                isRunning      = _viewModel.IsRunning,
                debugEnabled   = _viewModel.DebugEnabled,
                executionState = _viewModel.State
            });
        }

        // ----------------------------------------------------------------
        // Inspection handlers
        // ----------------------------------------------------------------

        /// <summary>GET /api/debug/snapshot — full controller snapshot for debugging.</summary>
        private async Task HandleGetDebugSnapshotAsync(HttpContext ctx)
        {
            var eu = _viewModel.GetExecutionUnit();
            await ctx.Response.WriteAsJsonAsync(new
            {
                isRunning    = _viewModel.IsRunning,
                debugEnabled = _viewModel.DebugEnabled,
                state        = eu.State,
                debugInfo    = eu.DebugInfo,
                stackInfo    = eu.StackInfo,
                memoryInfo   = eu.MemoryInfo,
                countersInfo = eu.CountersInfo,
                timersInfo   = eu.TimersInfo,
                inputsInfo   = eu.InputsInfo,
                outputsInfo  = eu.OutputsInfo,
                currentLine  = eu.CurrentExecutionLine != null ? new
                {
                    lineNumber   = eu.CurrentExecutionLine.LineNumber,
                    source       = eu.CurrentExecutionLine.Source,
                    isComment    = eu.CurrentExecutionLine.IsComment,
                    hasError     = eu.CurrentExecutionLine.HasError,
                    errorMessage = eu.CurrentExecutionLine.ErrorMessage
                } : null,
                hasParseError          = eu.HasParseError,
                parseErrorMessage      = eu.ParseErrorMessage,
                hasExecutionError      = eu.HasExecutionError,
                executionErrorMessage  = eu.ExecutionErrorMessage
            });
        }

        /// <summary>GET /api/inputs — all input channel states.</summary>
        private async Task HandleGetInputsAsync(HttpContext ctx)
        {
            var eu = _viewModel.GetExecutionUnit();
            var inputs = new List<object>(eu.Inputs.Length);
            for (int i = 0; i < eu.Inputs.Length; i++)
                inputs.Add(new { index = i, value = eu.Inputs.GetValue(i), label = eu.Inputs[i].Label });

            await ctx.Response.WriteAsJsonAsync(new { count = eu.Inputs.Length, inputs });
        }

        /// <summary>GET /api/inputs/{index} — single input channel state.</summary>
        private async Task HandleGetInputByIndexAsync(HttpContext ctx)
        {
            var eu = _viewModel.GetExecutionUnit();
            if (!TryParseIndex(ctx, eu.Inputs.Length, out var index))
                return;

            await ctx.Response.WriteAsJsonAsync(new
            {
                index,
                value = eu.Inputs.GetValue(index),
                label = eu.Inputs[index].Label
            });
        }

        /// <summary>POST /api/inputs/{index}/toggle — toggles a modifiable input channel.</summary>
        private async Task HandleToggleInputAsync(HttpContext ctx)
        {
            var eu = _viewModel.GetExecutionUnit();
            if (!TryParseIndex(ctx, eu.Inputs.Length, out var index))
                return;

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (eu.Inputs[index] is Switch sw && sw.Modifiable)
                {
                    sw.Toggle();
                    await ctx.Response.WriteAsJsonAsync(new
                    {
                        success = true, index,
                        value = eu.Inputs.GetValue(index),
                        label = eu.Inputs[index].Label
                    });
                }
                else
                {
                    ctx.Response.StatusCode = 400;
                    await ctx.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        error = $"Input {index} ist nicht umschaltbar (kein Switch oder nicht modifizierbar)"
                    });
                }
            });
            System.Diagnostics.Debug.WriteLine($"[API] POST /api/inputs/{index}/toggle");
        }

        /// <summary>PUT /api/inputs/{index}/label — renames an input channel label.</summary>
        private async Task HandleSetInputLabelAsync(HttpContext ctx)
        {
            var eu = _viewModel.GetExecutionUnit();
            if (!TryParseIndex(ctx, eu.Inputs.Length, out var index))
                return;

            using var reader = new StreamReader(ctx.Request.Body);
            var newLabel = (await reader.ReadToEndAsync()).Trim();

            if (string.IsNullOrEmpty(newLabel))
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.WriteAsJsonAsync(new { error = "Label darf nicht leer sein." });
                return;
            }

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                eu.Inputs[index].Label = newLabel;
                if (index < _viewModel.Inputs.Count)
                    _viewModel.Inputs[index].Label = newLabel;

                await ctx.Response.WriteAsJsonAsync(new { success = true, index, label = newLabel });
            });
            System.Diagnostics.Debug.WriteLine($"[API] PUT /api/inputs/{index}/label → \"{newLabel}\"");
        }

        /// <summary>PUT /api/outputs/{index}/label — renames an output channel label.</summary>
        private async Task HandleSetOutputLabelAsync(HttpContext ctx)
        {
            var eu = _viewModel.GetExecutionUnit();
            if (!TryParseIndex(ctx, eu.Outputs.Length, out var index))
                return;

            using var reader = new StreamReader(ctx.Request.Body);
            var newLabel = (await reader.ReadToEndAsync()).Trim();

            if (string.IsNullOrEmpty(newLabel))
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.WriteAsJsonAsync(new { error = "Label darf nicht leer sein." });
                return;
            }

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                eu.Outputs[index].Label = newLabel;
                if (index < _viewModel.Outputs.Count)
                    _viewModel.Outputs[index].Label = newLabel;

                await ctx.Response.WriteAsJsonAsync(new { success = true, index, label = newLabel });
            });
            System.Diagnostics.Debug.WriteLine($"[API] PUT /api/outputs/{index}/label → \"{newLabel}\"");
        }

        /// <summary>GET /api/outputs — all output channel states.</summary>
        private async Task HandleGetOutputsAsync(HttpContext ctx)
        {
            var eu = _viewModel.GetExecutionUnit();
            var outputs = new List<object>(eu.Outputs.Length);
            for (int i = 0; i < eu.Outputs.Length; i++)
                outputs.Add(new { index = i, value = eu.Outputs.GetValue(i), label = eu.Outputs[i].Label });

            await ctx.Response.WriteAsJsonAsync(new { count = eu.Outputs.Length, outputs });
        }

        /// <summary>GET /api/memory?from=0&amp;to=63 — memory cell values in the requested range.</summary>
        private async Task HandleGetMemoryAsync(HttpContext ctx)
        {
            var eu = _viewModel.GetExecutionUnit();
            var (from, to) = ParseRangeQuery(ctx, eu.MemoryLength - 1, defaultTo: Math.Min(63, eu.MemoryLength - 1));

            var values = new List<object>(to - from + 1);
            for (int i = from; i <= to; i++)
                values.Add(new { index = i, value = eu.GetMemoryValue(i) });

            await ctx.Response.WriteAsJsonAsync(new { totalSize = eu.MemoryLength, from, to, values });
        }

        /// <summary>GET /api/timers?from=0&amp;to=15 — timer boolean states in the requested range.</summary>
        private async Task HandleGetTimersAsync(HttpContext ctx)
        {
            var eu = _viewModel.GetExecutionUnit();
            var (from, to) = ParseRangeQuery(ctx, eu.TimerLength - 1, defaultTo: Math.Min(15, eu.TimerLength - 1));

            var values = new List<object>(to - from + 1);
            for (int i = from; i <= to; i++)
                values.Add(new { index = i, value = eu.GetTimerValue(i) });

            await ctx.Response.WriteAsJsonAsync(new { totalCount = eu.TimerLength, from, to, values });
        }

        /// <summary>GET /api/counters?from=0&amp;to=15 — counter integer values in the requested range.</summary>
        private async Task HandleGetCountersAsync(HttpContext ctx)
        {
            var eu = _viewModel.GetExecutionUnit();
            var (from, to) = ParseRangeQuery(ctx, eu.Counters.Length - 1, defaultTo: Math.Min(15, eu.Counters.Length - 1));

            var values = new List<object>(to - from + 1);
            for (int i = from; i <= to; i++)
                values.Add(new { index = i, value = eu.Counters.GetValue(i) });

            await ctx.Response.WriteAsJsonAsync(new { totalCount = eu.Counters.Length, from, to, values });
        }

        /// <summary>GET /api/cycletime — current cycle time in milliseconds.</summary>
        private async Task HandleGetCycleTimeAsync(HttpContext ctx)
        {
            var eu = _viewModel.GetExecutionUnit();
            await ctx.Response.WriteAsJsonAsync(new { cycleTimeMs = eu.CycleTimeMs });
        }

        /// <summary>POST /api/cycletime — sets a new cycle time (minimum 1 ms).</summary>
        private async Task HandleSetCycleTimeAsync(HttpContext ctx)
        {
            using var reader = new StreamReader(ctx.Request.Body);
            var body = await reader.ReadToEndAsync();

            if (!int.TryParse(body, out var cycleTimeMs) || cycleTimeMs < 1)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.WriteAsJsonAsync(new { error = "Ungültiger Wert. Erwartet: positive Ganzzahl (Millisekunden, min. 1)" });
                return;
            }

            var eu = _viewModel.GetExecutionUnit();
            eu.CycleTimeMs = cycleTimeMs;
            await ctx.Response.WriteAsJsonAsync(new { success = true, cycleTimeMs = eu.CycleTimeMs });
            System.Diagnostics.Debug.WriteLine($"[API] POST /api/cycletime → {cycleTimeMs}ms");
        }

        /// <summary>POST /api/reset/outputs — resets all output channels to false.</summary>
        private async Task HandleResetOutputsAsync(HttpContext ctx)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var eu = _viewModel.GetExecutionUnit();
                for (int i = 0; i < eu.Outputs.Length; i++)
                    eu.Outputs[i].Value = false;

                await ctx.Response.WriteAsJsonAsync(new { success = true, message = "Alle Outputs zurückgesetzt" });
            });
            System.Diagnostics.Debug.WriteLine("[API] POST /api/reset/outputs");
        }

        // ----------------------------------------------------------------
        // Private helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Tries to parse the route value "index" and validates it against [0, <paramref name="maxExclusive"/>).
        /// Writes a 400 response and returns false when validation fails.
        /// </summary>
        private static bool TryParseIndex(HttpContext ctx, int maxExclusive, out int index)
        {
            if (!int.TryParse(ctx.Request.RouteValues["index"]?.ToString(), out index)
                || index < 0 || index >= maxExclusive)
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.WriteAsJsonAsync(new { error = $"Ungültiger Index. Gültig: 0-{maxExclusive - 1}" })
                   .GetAwaiter().GetResult();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Reads optional "from" and "to" query parameters and clamps them to [0, <paramref name="maxIndex"/>].
        /// </summary>
        private static (int from, int to) ParseRangeQuery(HttpContext ctx, int maxIndex, int defaultTo)
        {
            int from = 0, to = defaultTo;
            if (ctx.Request.Query.ContainsKey("from")) int.TryParse(ctx.Request.Query["from"], out from);
            if (ctx.Request.Query.ContainsKey("to"))   int.TryParse(ctx.Request.Query["to"],   out to);
            from = Math.Clamp(from, 0, maxIndex);
            to   = Math.Clamp(to,   from, maxIndex);
            return (from, to);
        }

        // ----------------------------------------------------------------
        // IDisposable
        // ----------------------------------------------------------------

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Stops the web host and releases managed resources.</summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
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
