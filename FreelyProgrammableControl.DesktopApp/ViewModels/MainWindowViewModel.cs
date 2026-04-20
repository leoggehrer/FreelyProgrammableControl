using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreelyProgrammableControl.DesktopApp.Services;
using FreelyProgrammableControl.Logic.Execution;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// Main window view model for the FPC desktop application.
    /// Coordinates the FPC execution engine, the HTTP API service, I/O device panels,
    /// the source editor (with undo/redo), cloud storage (Google Drive / n8n), and debug stepping.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private const int IOPageSize = 20;

        #region fields
        private bool isInitialized;
        private IClipboard? clipboard;
        private IStorageProvider? storageProvider;
        private Window? ownerWindow;
        private ApiService? apiService;
        private readonly string fpcSampleListFolderName;
        private readonly N8nWebhookService n8nWebhookService;
        private string? selectedFile;
        private readonly Stack<string> undoStack = new();
        private readonly Stack<string> redoStack = new();
        private string lastSourceText = string.Empty;
        private string saveUserinput = string.Empty;
        private readonly ExecutionUnit executionUnit;
        #endregion fields

        #region properties
        /// <summary>All input device view models (full list, independent of paging).</summary>
        public ObservableCollection<InputDeviceViewModel> Inputs { get; } = new();

        /// <summary>All output device view models (full list, independent of paging).</summary>
        public ObservableCollection<OutputDeviceViewModel> Outputs { get; } = new();

        /// <summary>The subset of <see cref="Inputs"/> shown on the currently selected input page.</summary>
        public ObservableCollection<InputDeviceViewModel> VisibleInputs { get; } = new();

        /// <summary>The subset of <see cref="Outputs"/> shown on the currently selected output page.</summary>
        public ObservableCollection<OutputDeviceViewModel> VisibleOutputs { get; } = new();

        /// <summary>Page-range labels for the input pager (e.g. "1-20", "21-40").</summary>
        public ObservableCollection<string> InputPageLabels { get; } = new();

        /// <summary>Page-range labels for the output pager (e.g. "1-20", "21-40").</summary>
        public ObservableCollection<string> OutputPageLabels { get; } = new();

        /// <summary>FPC source code shown in the editor. Automatically uppercased on change.</summary>
        [ObservableProperty]
        private string sourceText = string.Empty;

        /// <summary>Parse result or execution output text shown in the output panel.</summary>
        [ObservableProperty]
        private string outputText = string.Empty;

        /// <summary>Full raw execution state string from the engine (split into Parts 1-5 for display).</summary>
        [ObservableProperty]
        private string executionState = string.Empty;

        /// <summary>General execution state info (registers, current line). Panel 1 of 5.</summary>
        [ObservableProperty]
        private string executionStatePart1 = string.Empty;

        /// <summary>Boolean stack state. Panel 2 of 5. Empty when debug mode is off.</summary>
        [ObservableProperty]
        private string executionStatePart2 = string.Empty;

        /// <summary>Memory (M0-M63) state. Panel 3 of 5. Empty when debug mode is off.</summary>
        [ObservableProperty]
        private string executionStatePart3 = string.Empty;

        /// <summary>Counter values. Panel 4 of 5. Empty when debug mode is off.</summary>
        [ObservableProperty]
        private string executionStatePart4 = string.Empty;

        /// <summary>Timer states. Panel 5 of 5. Empty when debug mode is off.</summary>
        [ObservableProperty]
        private string executionStatePart5 = string.Empty;

        /// <summary>Status bar text (current file path, API port, error messages).</summary>
        [ObservableProperty]
        private string statusText = string.Empty;

        /// <summary><c>true</c> while the program is running — makes the source editor read-only.</summary>
        [ObservableProperty]
        private bool isSourceReadOnly;

        /// <summary>
        /// Controls whether the debug-mode toggle button is enabled in the UI.
        /// Set to <c>true</c> only when the engine is stopped (you cannot switch debug mode while running).
        /// </summary>
        [ObservableProperty]
        private bool isDebugEnabled = true;

        /// <summary>
        /// Whether debug mode is currently active in the execution engine.
        /// Toggling this writes through to <see cref="ExecutionUnit.DebugEnabled"/>.
        /// </summary>
        [ObservableProperty]
        private bool debugEnabled;

        /// <summary>Label shown on the debug toggle button ("Debug: ON" / "Debug: OFF").</summary>
        [ObservableProperty]
        private string debugButtonText = "Debug: OFF";

        /// <summary>Zero-based line index the source-editor scrolls to (used for debug stepping).</summary>
        [ObservableProperty]
        private int currentLineNumber = 0;

        /// <summary>Highest valid line index in the current source text.</summary>
        [ObservableProperty]
        private int maxLineNumber = 0;

        /// <summary>Lowest valid line index (always 0).</summary>
        [ObservableProperty]
        private int minLineNumber = 0;

        /// <summary>Zero-based index of the currently selected input page.</summary>
        [ObservableProperty]
        private int selectedInputPageIndex;

        /// <summary>Zero-based index of the currently selected output page.</summary>
        [ObservableProperty]
        private int selectedOutputPageIndex;

        /// <summary><c>true</c> when the input list spans more than one page.</summary>
        [ObservableProperty]
        private bool hasMultipleInputPages;

        /// <summary><c>true</c> when the output list spans more than one page.</summary>
        [ObservableProperty]
        private bool hasMultipleOutputPages;

        /// <summary>Number of input channels configured for the execution engine.</summary>
        public int InputCount => executionUnit.Inputs.Length;

        /// <summary>Number of output channels configured for the execution engine.</summary>
        public int OutputCount => executionUnit.Outputs.Length;

        /// <summary><c>true</c> while the FPC program is executing.</summary>
        public bool IsRunning => executionUnit.IsRunning;

        /// <summary><c>true</c> when the last loaded source contained parse errors.</summary>
        public bool HasParseError => executionUnit.HasParseError;

        /// <summary>Parse error message from the last load attempt, or <c>null</c> if none.</summary>
        public string? ParseErrorMessage => executionUnit.ParseErrorMessage;

        /// <summary>The source lines currently loaded in the execution engine.</summary>
        public string[] Source => executionUnit.Source;

        /// <summary>Human-readable engine state summary string.</summary>
        public string State => executionUnit.State;

        /// <summary>
        /// Gibt die ExecutionUnit für API-Zugriffe zurück.
        /// </summary>
        public ExecutionUnit GetExecutionUnit() => executionUnit;

        /// <summary>
        /// Startet das Programm synchron ohne UI-Dialoge (für API-Aufrufe).
        /// </summary>
        /// <returns>Tuple mit Erfolg und optionaler Fehlermeldung.</returns>
        public (bool Success, string? ErrorMessage) StartForApi()
        {
            StatusText = selectedFile ?? string.Empty;

            if (executionUnit.IsRunning)
                return (false, "Programm läuft bereits.");

            try
            {
                var sourceTextToRun = string.IsNullOrWhiteSpace(SourceText)
                    ? string.Join(Environment.NewLine, executionUnit.Source ?? Array.Empty<string>())
                    : SourceText;

                if (string.IsNullOrWhiteSpace(sourceTextToRun))
                    return (false, "Kein Programm geladen (SourceText ist leer).");

                var source = sourceTextToRun.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
                var errors = ParseAndView(source);

                if (errors > 0)
                {
                    var parsedLines = executionUnit.Parse(source);
                    var errorMessages = parsedLines.Where(pl => pl.HasError)
                        .Select(pl => $"Zeile {pl.LineNumber}: {pl.ErrorMessage}")
                        .ToList();

                    return (false, $"{errors} Parse-Fehler: {string.Join("; ", errorMessages)}");
                }

                saveUserinput = sourceTextToRun;

                executionUnit.LoadSource(source);
                executionUnit.Start();

                CurrentLineNumber = executionUnit.CurrentExecutionLine?.LineNumber ?? 0;
                UpdateRunState();

                return executionUnit.IsRunning
                    ? (true, null)
                    : (false, "Programm konnte nicht gestartet werden.");
            }
            catch (Exception ex)
            {
                return (false, $"Fehler beim Starten: {ex.Message}");
            }
        }

        /// <summary>
        /// Stoppt das Programm synchron ohne UI-Dialoge (für API-Aufrufe).
        /// </summary>
        public (bool Success, string? ErrorMessage) StopForApi()
        {
            if (!executionUnit.IsRunning)
                return (false, "Programm läuft nicht.");

            try
            {
                executionUnit.Stop();
                SourceText = saveUserinput;
                saveUserinput = string.Empty;
                UpdateRunState();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Fehler beim Stoppen: {ex.Message}");
            }
        }
        #endregion properties

        #region constructors
        /// <summary>
        /// Creates the view model, wires up the execution engine callbacks,
        /// loads the last-used program file from disk, and starts the HTTP API service.
        /// </summary>
        public MainWindowViewModel()
        {
            var settings = ConfigurationHelper.GetSettings();

            var configuredInputCount = Math.Max(1, settings.Machine.InputCount);
            var configuredOutputCount = Math.Max(1, settings.Machine.OutputCount);

            executionUnit = new ExecutionUnit(configuredInputCount, configuredOutputCount);
            //            executionUnit.Inputs[0] = new Blinker(new TimeSpan(0, 0, 0, 0, 1000)) { Label = "Flasher 0" };
            selectedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "newProgram.fpc");
            StatusText = selectedFile ?? string.Empty;

            if (File.Exists(selectedFile))
            {
                var lines = File.ReadAllLines(selectedFile);

                SourceText = lines.Length > 0 ? lines.Aggregate((a, b) => $"{a}{Environment.NewLine}{b}") : string.Empty;
                executionUnit.LoadSource(lines);
            }

            executionUnit.Attach(UpdateExecutionState!);
            executionUnit.Inputs.Attach(OnUpdateInputs!);
            executionUnit.Outputs.Attach(OnUpdateOutputs!);

            n8nWebhookService = new N8nWebhookService();
            fpcSampleListFolderName = settings.N8N.FPCSampleListFolderName ?? string.Empty;

            CreateInputItems();
            CreateOutputItems();

            // Start API Service
            StartApiService(settings);
        }
        #endregion constructors

        #region methods
        /// <summary>
        /// Starts the Kestrel HTTP API service on the configured port.
        /// Updates <see cref="StatusText"/> on success or failure.
        /// </summary>
        private async void StartApiService(AppSettings settings)
        {
            try
            {
                apiService = new ApiService(this, settings.Api.Port);
                await apiService.StartAsync();
                StatusText = $"{selectedFile} - API läuft auf Port {apiService.Port}";
            }
            catch (Exception ex)
            {
                StatusText = $"API-Server konnte nicht gestartet werden: {ex.Message}";
            }
        }
        /// <summary>
        /// Binds platform-specific services (file picker, clipboard) to this view model.
        /// Must be called once from the code-behind after the window is loaded.
        /// Subsequent calls are no-ops.
        /// </summary>
        /// <param name="provider">Avalonia storage provider for file dialogs.</param>
        /// <param name="owner">The main application window.</param>
        public void Initialize(IStorageProvider? provider, Window owner)
        {
            if (isInitialized)
            {
                return;
            }

            storageProvider = provider;
            ownerWindow = owner;
            clipboard = owner.Clipboard;
            isInitialized = true;

            // Recreate inputs and outputs with window reference
            CreateInputItems();
            CreateOutputItems();
        }

        /// <summary>Clears the editor and resets the current file to the default new-program path.</summary>
        [RelayCommand]
        private void New()
        {
            SourceText = string.Empty;
            selectedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "newProgram.fpc");
            StatusText = selectedFile;
        }

        /// <summary>Opens a file picker and loads the selected .fpc file into the editor.</summary>
        [RelayCommand(CanExecute = nameof(CanOpen))]
        private async Task OpenAsync()
        {
            if (storageProvider is null)
            {
                await ShowErrorDialogAsync(ownerWindow, "Fehler", "Dateisystem nicht verfügbar.");
                return;
            }

            try
            {
                var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    AllowMultiple = false,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("All files") { Patterns = ["*"] },
                        new FilePickerFileType("Program files") { Patterns = ["*.fpc"] }
                    ]
                });

                if (result.Count == 1)
                {
                    selectedFile = result[0].Path.LocalPath;
                    SourceText = await File.ReadAllTextAsync(selectedFile);
                    StatusText = selectedFile;
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync(ownerWindow, "Fehler beim Öffnen", $"Die Datei konnte nicht geöffnet werden:\n{ex.Message}");
            }
        }
        private bool CanOpen()
        {
            return executionUnit.IsRunning == false;
        }

        /// <summary>Saves the current source text to the current file path (no dialog).</summary>
        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            if (selectedFile != null)
            {
                try
                {
                    await File.WriteAllTextAsync(selectedFile, SourceText ?? string.Empty);
                    StatusText = $"{selectedFile} - Gespeichert";
                }
                catch (Exception ex)
                {
                    await ShowErrorDialogAsync(ownerWindow, "Fehler beim Speichern", $"Die Datei konnte nicht gespeichert werden:\n{ex.Message}");
                }
            }
        }

        private bool CanSave()
        {
            return executionUnit.IsRunning == false;
        }

        /// <summary>Opens a Save As dialog and saves the source to the chosen path.</summary>
        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsAsync()
        {
            if (storageProvider is null)
            {
                await ShowErrorDialogAsync(ownerWindow, "Fehler", "Dateisystem nicht verfügbar.");
                return;
            }

            if (!storageProvider.CanSave)
            {
                await ShowErrorDialogAsync(ownerWindow, "Nicht unterstützt", "Speichern wird auf dieser Plattform nicht unterstützt.");
                return;
            }

            try
            {
                var saveOptions = new FilePickerSaveOptions
                {
                    Title = "Save As...",
                    FileTypeChoices =
                    [
                        new FilePickerFileType("All files") { Patterns = ["*"] },
                        new FilePickerFileType("Program files") { Patterns = ["*.fpc"] }
                    ],
                    SuggestedFileName = Path.GetFileName(selectedFile),
                    SuggestedStartLocation = selectedFile is null ? null : await storageProvider.TryGetFolderFromPathAsync(selectedFile)
                };

                var result = await storageProvider.SaveFilePickerAsync(saveOptions);

                if (result != null)
                {
                    string content = SourceText ?? string.Empty;
                    await File.WriteAllTextAsync(result.Path.LocalPath, content);
                    selectedFile = result.Path.LocalPath;
                    StatusText = $"{selectedFile} - Gespeichert";
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync(ownerWindow, "Fehler beim Speichern", $"Die Datei konnte nicht gespeichert werden:\n{ex.Message}");
            }
        }

        private bool CanSaveToGoogleDrive()
        {
            return executionUnit.IsRunning == false && !string.IsNullOrWhiteSpace(SourceText);
        }

        /// <summary>Prompts for a filename and uploads the current source to Google Drive via n8n webhook.</summary>
        [RelayCommand(CanExecute = nameof(CanSaveToGoogleDrive))]
        private async Task SaveToGoogleDriveAsync()
        {
            try
            {
                var suggestedFilename = string.IsNullOrWhiteSpace(selectedFile)
                    ? "newProgram.fpc"
                    : Path.GetFileName(selectedFile);

                var filename = await PromptFilenameAsync(suggestedFilename);

                if (string.IsNullOrWhiteSpace(filename))
                {
                    StatusText = "Speichern zu Google Drive abgebrochen";
                    return;
                }

                await n8nWebhookService.SaveToGoogleDriveAsync(filename, SourceText ?? string.Empty);
                StatusText = $"{filename} - An Google Drive gesendet";
            }
            catch (InvalidOperationException ex)
            {
                await ShowErrorDialogAsync(ownerWindow, "n8n Konfiguration fehlt", ex.Message);
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync(ownerWindow, "Fehler beim n8n-Aufruf", $"Der Webhook konnte nicht aufgerufen werden:\n{ex.Message}");
            }
        }

        /// <summary>Saves the current source to the n8n PGVector store for AI retrieval.</summary>
        [RelayCommand(CanExecute = nameof(CanSaveToVektor))]
        private async Task SaveToVektorAsync()
        {
            try
            {
                StatusText = "Programm wird in Vector Store gespeichert...";
                await n8nWebhookService.SaveToVektorAsync();
                StatusText = "Programm erfolgreich im Vector Store gespeichert";
                await ShowInfoDialogAsync(ownerWindow, "Vector Store", "Das Programm wurde erfolgreich im Vector Store gespeichert.");
            }
            catch (InvalidOperationException ex)
            {
                await ShowErrorDialogAsync(ownerWindow, "n8n Konfiguration fehlt", ex.Message);
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync(ownerWindow, "Fehler beim Speichern", $"Programm konnte nicht im Vector Store gespeichert werden:\n{ex.Message}");
            }
        }

        private bool CanSaveToVektor()
        {
            return !string.IsNullOrWhiteSpace(SourceText) && !executionUnit.IsRunning;
        }

        private bool CanLoadFromGoogleDrive()
        {
            return executionUnit.IsRunning == false;
        }

        /// <summary>Fetches the file list from Google Drive and loads the user-selected program into the editor.</summary>
        [RelayCommand(CanExecute = nameof(CanLoadFromGoogleDrive))]
        private async Task LoadFromGoogleDriveAsync()
        {
            if (executionUnit.IsRunning)
            {
                StatusText = "Laden aus Google Drive nur im gestoppten Zustand möglich";
                return;
            }

            try
            {
                var samples = await n8nWebhookService.GetFPCSampleListAsync(fpcSampleListFolderName);

                if (samples.Count == 0)
                {
                    StatusText = "Keine Dateien in Google Drive gefunden";
                    return;
                }

                var selectedSample = await PromptFpcSampleSelectionAsync(samples);
                if (selectedSample == null)
                {
                    StatusText = "Laden aus Google Drive abgebrochen";
                    return;
                }

                var loadResult = await n8nWebhookService.LoadFPCSampleAsync(selectedSample.Id);
                SourceText = loadResult.Source;
                selectedFile = selectedSample.Name;
                var loadSourceLabel = loadResult.LoadSource == FpcSampleLoadSource.PrimaryWebhook
                    ? "Primary"
                    : "Fallback";
                StatusText = $"{selectedSample.Name} - Von Google Drive geladen ({loadSourceLabel})";
            }
            catch (InvalidOperationException ex)
            {
                await ShowErrorDialogAsync(ownerWindow, "n8n Konfiguration fehlt", ex.Message);
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync(ownerWindow, "Fehler beim Laden", $"Datei konnte nicht aus Google Drive geladen werden:\n{ex.Message}");
            }
        }

        /// <summary>Disposes the API service and closes the main window.</summary>
        [RelayCommand]
        private void Exit()
        {
            // Cleanup API Service
            apiService?.Dispose();
            apiService = null;

            ownerWindow?.Close();
        }

        /// <summary>Reverts the source editor to the previous text in the undo history.</summary>
        [RelayCommand(CanExecute = nameof(CanUndo))]
        private void Undo()
        {
            if (undoStack.Count > 0)
            {
                redoStack.Push(SourceText);
                var previousText = undoStack.Pop();
                lastSourceText = previousText; // Prevent adding to undo stack
                SourceText = previousText;
                UndoCommand?.NotifyCanExecuteChanged();
                RedoCommand?.NotifyCanExecuteChanged();
            }
        }

        private bool CanUndo() => undoStack.Count > 0 && !IsSourceReadOnly;

        /// <summary>Reapplies the most recently undone source edit.</summary>
        [RelayCommand(CanExecute = nameof(CanRedo))]
        private void Redo()
        {
            if (redoStack.Count > 0)
            {
                undoStack.Push(SourceText);
                var nextText = redoStack.Pop();
                lastSourceText = nextText; // Prevent adding to undo stack
                SourceText = nextText;
                UndoCommand?.NotifyCanExecuteChanged();
                RedoCommand?.NotifyCanExecuteChanged();
            }
        }

        private bool CanRedo() => redoStack.Count > 0 && !IsSourceReadOnly;

        /// <summary>Copies the entire source text to the system clipboard.</summary>
        [RelayCommand(CanExecute = nameof(CanCopyOrCut))]
        private async Task CopyAsync()
        {
            if (clipboard != null && !string.IsNullOrEmpty(SourceText))
            {
                await clipboard.SetTextAsync(SourceText);
            }
        }

        private bool CanCopyOrCut() => !string.IsNullOrEmpty(SourceText);

        /// <summary>Replaces the source text with the clipboard contents.</summary>
        [RelayCommand(CanExecute = nameof(CanPasteCommand))]
        private async Task PasteAsync()
        {
            if (clipboard != null)
            {
                var text = await clipboard.GetTextAsync();
                if (!string.IsNullOrEmpty(text))
                {
                    SourceText = text;
                }
            }
        }

        private bool CanPasteCommand() => !IsSourceReadOnly;

        /// <summary>Copies the source to the clipboard and then clears the editor.</summary>
        [RelayCommand(CanExecute = nameof(CanCopyOrCut))]
        private async Task CutAsync()
        {
            if (clipboard != null && !string.IsNullOrEmpty(SourceText))
            {
                await clipboard.SetTextAsync(SourceText);
                SourceText = string.Empty;
            }
        }

        /// <summary>Parses the source, loads it into the engine, and starts execution. Shows an error dialog on parse failure.</summary>
        [RelayCommand(CanExecute = nameof(CanStart))]
        private async Task StartAsync()
        {
            StatusText = selectedFile ?? string.Empty;

            if (executionUnit.IsRunning == false && string.IsNullOrWhiteSpace(SourceText) == false)
            {
                try
                {
                    var source = SourceText.Split(Environment.NewLine);
                    var errors = ParseAndView(source);

                    if (errors == 0)
                    {
                        saveUserinput = SourceText;

                        executionUnit.LoadSource(source);
                        executionUnit.Start();

                        SourceText = ExecutionUnit.PrepareSource(source)
                                                  .Select((i, l) => $"{l:d4}: {i}")
                                                  .Aggregate((a, b) => $"{a}{Environment.NewLine}{b}");

                        CurrentLineNumber = executionUnit.CurrentExecutionLine?.LineNumber ?? 0;
                        UpdateRunState();
                    }
                    else
                    {
                        await ShowErrorDialogAsync(ownerWindow, "Parse-Fehler", $"Das Programm enthält {errors} Fehler und kann nicht gestartet werden.");
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorDialogAsync(ownerWindow, "Fehler beim Start", $"Das Programm konnte nicht gestartet werden:\n{ex.Message}");
                }
            }
        }

        private bool CanStart()
        {
            return executionUnit.IsRunning == false;
        }

        /// <summary>Parses and loads the current source into the engine without starting execution.</summary>
        [RelayCommand(CanExecute = nameof(CanLoadSource))]
        private void LoadSource()
        {
            if (executionUnit.IsRunning == false && string.IsNullOrWhiteSpace(SourceText) == false)
            {
                var source = SourceText.Split(Environment.NewLine);
                var errors = ParseAndView(source);

                if (errors == 0)
                {
                    executionUnit.LoadSource(source);

                    UpdateRunState();
                }
            }
        }

        private bool CanLoadSource()
        {
            return executionUnit.IsRunning == false;
        }

        /// <summary>Stops the running program and restores the editable source text in the editor.</summary>
        [RelayCommand(CanExecute = nameof(CanStop))]
        private void Stop()
        {
            if (executionUnit.IsRunning)
            {
                executionUnit.Stop();
                SourceText = saveUserinput;
                saveUserinput = string.Empty;
            }
            UpdateRunState();
        }

        private bool CanStop()
        {
            return executionUnit.IsRunning;
        }

        /// <summary>Parses the current source and shows the annotated result in the output panel.</summary>
        [RelayCommand(CanExecute = nameof(CanParse))]
        private void Parse()
        {
            ParseAndView(SourceText.Split(Environment.NewLine));
        }

        private bool CanParse()
        {
            return executionUnit.IsRunning == false;
        }

        /// <summary>
        /// Advances execution by one instruction in debug mode.
        /// If the engine has jumped past <see cref="CurrentLineNumber"/>, steps forward until it catches up.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanStep))]
        private void Step()
        {
            if (executionUnit.IsRunning
                && executionUnit.DebugEnabled
                && executionUnit.CurrentExecutionLine != null)
            {
                if (executionUnit.CurrentExecutionLine.LineNumber == CurrentLineNumber)
                {
                    executionUnit.Step();
                    if (executionUnit.CurrentExecutionLine != null)
                    {
                        CurrentLineNumber = executionUnit.CurrentExecutionLine.LineNumber;
                    }
                }
                else
                {
                    while (executionUnit.CurrentExecutionLine != null
                           && executionUnit.CurrentExecutionLine.LineNumber != CurrentLineNumber)
                    {
                        executionUnit.Step();
                    }
                }
            }
            UpdateRunState();
        }

        private bool CanStep()
        {
            return executionUnit.IsRunning && executionUnit.DebugEnabled;
        }

        /// <summary>Shows the About dialog with version and API status information.</summary>
        [RelayCommand]
        private async Task AboutAsync()
        {
            if (ownerWindow != null)
            {
                var aboutDialog = new Window
                {
                    Title = "About FreelyProgrammableControl",
                    Width = 500,
                    Height = 350,
                    CanResize = false,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = new StackPanel
                    {
                        Margin = new Avalonia.Thickness(20),
                        Spacing = 15,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Freely Programmable Control",
                                FontSize = 24,
                                FontWeight = Avalonia.Media.FontWeight.Bold,
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                            },
                            new TextBlock
                            {
                                Text = "Version 1.0.0",
                                FontSize = 16,
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                Foreground = Avalonia.Media.Brushes.Gray
                            },
                            new Separator { Margin = new Avalonia.Thickness(0, 10) },
                            new TextBlock
                            {
                                Text = "Eine flexible Steuerungssoftware f\u00fcr programmierbare Eingabe-/Ausgabelogik.",
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                FontSize = 14,
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                            },
                            new TextBlock
                            {
                                Text = "Features:",
                                FontSize = 14,
                                FontWeight = Avalonia.Media.FontWeight.Bold,
                                Margin = new Avalonia.Thickness(0, 10, 0, 5)
                            },
                            new TextBlock
                            {
                                Text = "\u2022 Programmierbare Steuerungslogik\\n\u2022 Echtzeit-Debugging\\n\u2022 HTTP-API f\u00fcr Remote-Steuerung\\n\u2022 Flexible Ein-/Ausgabekonfiguration",
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                FontSize = 12,
                                Margin = new Avalonia.Thickness(20, 0, 0, 0)
                            },
                            new Separator { Margin = new Avalonia.Thickness(0, 10) },
                            new TextBlock
                            {
                                Text = $"API-Server: {(apiService?.IsRunning == true ? $"L\u00e4uft auf Port {apiService.Port}" : "Gestoppt")}",
                                FontSize = 12,
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                Foreground = Avalonia.Media.Brushes.DarkGreen
                            },

                        }
                    }
                };
                await aboutDialog.ShowDialog(ownerWindow);
            }
        }

        /// <summary>
        /// Parses <paramref name="lines"/> and writes the annotated result to <see cref="OutputText"/>.
        /// </summary>
        /// <returns>Number of parse errors found.</returns>
        private int ParseAndView(string[] lines)
        {
            var parsedLines = executionUnit.Parse(lines);
            var parsedText = parsedLines.Select(pl => pl.ToString()).ToList();
            var errorCount = parsedLines.Count(pl => pl.HasError);

            parsedText.Insert(0, $"Text has {errorCount} Error(s)");
            parsedText.Insert(1, string.Empty);

            OutputText = string.Join(Environment.NewLine, parsedText);
            return errorCount;
        }

        /// <summary>
        /// Syncs UI-state properties with the engine state and notifies all commands
        /// that their <c>CanExecute</c> result may have changed.
        /// Call this after any operation that starts or stops the engine.
        /// </summary>
        public void UpdateRunState()
        {
            IsSourceReadOnly = executionUnit.IsRunning;
            IsDebugEnabled = !executionUnit.IsRunning;

            OpenCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            SaveAsCommand.NotifyCanExecuteChanged();
            SaveToGoogleDriveCommand.NotifyCanExecuteChanged();
            LoadFromGoogleDriveCommand.NotifyCanExecuteChanged();
            LoadSourceCommand.NotifyCanExecuteChanged();

            ParseCommand.NotifyCanExecuteChanged();
            StartCommand.NotifyCanExecuteChanged();
            StopCommand.NotifyCanExecuteChanged();
            StepCommand.NotifyCanExecuteChanged();
            
            UndoCommand?.NotifyCanExecuteChanged();
            RedoCommand?.NotifyCanExecuteChanged();
            CopyCommand?.NotifyCanExecuteChanged();
            PasteCommand?.NotifyCanExecuteChanged();
            CutCommand?.NotifyCanExecuteChanged();

            foreach (var input in Inputs)
            {
                input.NotifyCanExecuteChanged();
            }

            foreach (var output in Outputs)
            {
                output.NotifyCanExecuteChanged();
            }
        }

        /// <summary>Callback invoked by the engine when input device values change. Refreshes all input view models on the UI thread.</summary>
        private void OnUpdateInputs(object sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                for (int i = 0; i < Inputs.Count; i++)
                {
                    Inputs[i].UpdateFromDevice();
                }
            });
        }

        /// <summary>Callback invoked by the engine when output device values change. Refreshes all output view models on the UI thread.</summary>
        private void OnUpdateOutputs(object sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                for (int i = 0; i < Outputs.Count; i++)
                {
                    Outputs[i].UpdateFromDevice();
                }
            });
        }

        /// <summary>Callback invoked after each execution cycle. Updates <see cref="ExecutionState"/> from the engine.
        /// When a runtime error stops the execution, also updates <see cref="StatusText"/> and refreshes command states on the UI thread.</summary>
        private void UpdateExecutionState(object sender, EventArgs e)
        {
            ExecutionState = executionUnit.State;

            if (executionUnit.HasExecutionError)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SourceText = saveUserinput;
                    StatusText = $"Laufzeitfehler: {executionUnit.ExecutionErrorMessage}";
                    saveUserinput = string.Empty;
                    UpdateRunState();
                });
            }
        }

        /// <summary>Builds the <see cref="Inputs"/> collection from the execution engine and rebuilds the input pages.</summary>
        private void CreateInputItems()
        {
            Inputs.Clear();
            for (int i = 0; i < executionUnit.Inputs.Length; i++)
            {
                Inputs.Add(new InputDeviceViewModel(executionUnit.Inputs[i], ownerWindow, () => !executionUnit.IsRunning));
            }

            RebuildInputPages();
        }

        /// <summary>Builds the <see cref="Outputs"/> collection from the execution engine and rebuilds the output pages.</summary>
        private void CreateOutputItems()
        {
            Outputs.Clear();
            for (int i = 0; i < executionUnit.Outputs.Length; i++)
            {
                Outputs.Add(new OutputDeviceViewModel(executionUnit.Outputs[i], i, ownerWindow, () => !executionUnit.IsRunning));
            }

            RebuildOutputPages();
        }

        /// <summary>Regenerates <see cref="InputPageLabels"/> and resets paging state after the input list changes.</summary>
        private void RebuildInputPages()
        {
            InputPageLabels.Clear();

            var pageCount = Math.Max(1, (Inputs.Count + IOPageSize - 1) / IOPageSize);
            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                var start = pageIndex * IOPageSize + 1;
                var end = Math.Min((pageIndex + 1) * IOPageSize, Inputs.Count);
                InputPageLabels.Add($"{start}-{end}");
            }

            HasMultipleInputPages = pageCount > 1;

            if (SelectedInputPageIndex >= pageCount || SelectedInputPageIndex < 0)
            {
                SelectedInputPageIndex = 0;
            }

            RefreshVisibleInputs();
        }

        /// <summary>Regenerates <see cref="OutputPageLabels"/> and resets paging state after the output list changes.</summary>
        private void RebuildOutputPages()
        {
            OutputPageLabels.Clear();

            var pageCount = Math.Max(1, (Outputs.Count + IOPageSize - 1) / IOPageSize);
            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                var start = pageIndex * IOPageSize + 1;
                var end = Math.Min((pageIndex + 1) * IOPageSize, Outputs.Count);
                OutputPageLabels.Add($"{start}-{end}");
            }

            HasMultipleOutputPages = pageCount > 1;

            if (SelectedOutputPageIndex >= pageCount || SelectedOutputPageIndex < 0)
            {
                SelectedOutputPageIndex = 0;
            }

            RefreshVisibleOutputs();
        }

        /// <summary>Repopulates <see cref="VisibleInputs"/> with the items for the current input page.</summary>
        private void RefreshVisibleInputs()
        {
            VisibleInputs.Clear();

            if (Inputs.Count == 0)
            {
                return;
            }

            var safeIndex = Math.Clamp(SelectedInputPageIndex, 0, Math.Max(0, InputPageLabels.Count - 1));
            var start = safeIndex * IOPageSize;
            var endExclusive = Math.Min(start + IOPageSize, Inputs.Count);

            for (int i = start; i < endExclusive; i++)
            {
                Inputs[i].UpdateFromDevice();
                VisibleInputs.Add(Inputs[i]);
            }
        }

        /// <summary>Repopulates <see cref="VisibleOutputs"/> with the items for the current output page.</summary>
        private void RefreshVisibleOutputs()
        {
            VisibleOutputs.Clear();

            if (Outputs.Count == 0)
            {
                return;
            }

            var safeIndex = Math.Clamp(SelectedOutputPageIndex, 0, Math.Max(0, OutputPageLabels.Count - 1));
            var start = safeIndex * IOPageSize;
            var endExclusive = Math.Min(start + IOPageSize, Outputs.Count);

            for (int i = start; i < endExclusive; i++)
            {
                Outputs[i].UpdateFromDevice();
                VisibleOutputs.Add(Outputs[i]);
            }
        }

        /// <summary>
        /// Shows a selection dialog to choose one file from the Google Drive list.
        /// </summary>
        private async Task<FPCSampleListItem?> PromptFpcSampleSelectionAsync(IReadOnlyList<FPCSampleListItem> samples)
        {
            if (ownerWindow == null)
            {
                return null;
            }

            var sampleList = samples.ToList();

            var dialog = new Window
            {
                Title = "Datei aus Google Drive laden",
                Width = 500,
                Height = 220,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var comboBox = new ComboBox
            {
                ItemsSource = sampleList,
                SelectedIndex = sampleList.Count > 0 ? 0 : -1,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            };

            FPCSampleListItem? result = null;

            var okButton = new Button
            {
                Content = "Laden",
                Width = 100
            };
            okButton.Click += (s, e) =>
            {
                result = comboBox.SelectedItem as FPCSampleListItem;
                dialog.Close();
            };

            var cancelButton = new Button
            {
                Content = "Abbrechen",
                Width = 100
            };
            cancelButton.Click += (s, e) => dialog.Close();

            dialog.Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Bitte Datei aus Google Drive auswählen:",
                        FontSize = 14
                    },
                    comboBox,
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        Spacing = 8,
                        Children =
                        {
                            cancelButton,
                            okButton
                        }
                    }
                }
            };

            await dialog.ShowDialog(ownerWindow);
            return result;
        }

        /// <summary>
        /// Shows an input dialog to ask the user for a filename.
        /// </summary>
        private async Task<string?> PromptFilenameAsync(string suggestedFilename)
        {
            if (ownerWindow == null)
            {
                return null;
            }

            var dialog = new Window
            {
                Title = "Dateiname eingeben",
                Width = 450,
                Height = 190,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var filenameBox = new TextBox
            {
                Text = suggestedFilename,
                Watermark = "Dateiname (.fpc)",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            };

            string? result = null;

            var okButton = new Button
            {
                Content = "OK",
                Width = 100
            };
            okButton.Click += (s, e) =>
            {
                result = filenameBox.Text?.Trim();
                dialog.Close();
            };

            var cancelButton = new Button
            {
                Content = "Abbrechen",
                Width = 100
            };
            cancelButton.Click += (s, e) => dialog.Close();

            dialog.Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Bitte Dateinamen für Google Drive eingeben:",
                        FontSize = 14
                    },
                    filenameBox,
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        Spacing = 8,
                        Children =
                        {
                            cancelButton,
                            okButton
                        }
                    }
                }
            };

            await dialog.ShowDialog(ownerWindow);
            return result;
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
            if (disposing)
            {
                // Stop and dispose API service
                apiService?.Dispose();
                apiService = null;

                // Stop execution unit if running
                if (executionUnit?.IsRunning == true)
                {
                    executionUnit.Stop();
                }
            }
        }
        #endregion methods

        #region partial methods
        partial void OnSourceTextChanged(string value)
        {
            var lineCount = value?.Split(Environment.NewLine).Length ?? 0;

            MinLineNumber = 0;
            MaxLineNumber = lineCount > 0 ? lineCount - 1 : 0;

            if (CurrentLineNumber < MinLineNumber
                || CurrentLineNumber > MaxLineNumber
                || lineCount == 0)
            {
                CurrentLineNumber = MinLineNumber;
            }

            // Konvertiere Text zu Großbuchstaben wenn sich der Wert ändert
            if (value != null && value != value.ToUpper())
            {
                SourceText = value.ToUpper();
                return;
            }

            // Undo/Redo Stack Management
            if (!string.IsNullOrEmpty(lastSourceText) && lastSourceText != value)
            {
                undoStack.Push(lastSourceText);
                if (undoStack.Count > 50) // Limit stack size
                {
                    var temp = undoStack.Reverse().Take(50).Reverse().ToList();
                    undoStack.Clear();
                    foreach (var item in temp)
                        undoStack.Push(item);
                }
                redoStack.Clear();
                UndoCommand?.NotifyCanExecuteChanged();
                RedoCommand?.NotifyCanExecuteChanged();
            }

            lastSourceText = value ?? string.Empty;
            if (executionUnit.IsRunning == false)
            {
                ParseAndView(SourceText.Split(Environment.NewLine));
            }
        }

        partial void OnExecutionStateChanged(string value)
        {
            ExecutionStatePart1 = value ?? string.Empty;
            ExecutionStatePart2 = DebugEnabled ? executionUnit.StackInfo : string.Empty;
            ExecutionStatePart3 = DebugEnabled ? executionUnit.MemoryInfo : string.Empty;
            ExecutionStatePart4 = DebugEnabled ? executionUnit.CountersInfo : string.Empty;
            ExecutionStatePart5 = DebugEnabled ? executionUnit.TimersInfo : string.Empty;
        }

        partial void OnDebugEnabledChanged(bool value)
        {
            executionUnit.DebugEnabled = value;
            DebugButtonText = value ? "Debug: ON" : "Debug: OFF";
        }

        partial void OnSelectedInputPageIndexChanged(int value)
        {
            RefreshVisibleInputs();
        }

        partial void OnSelectedOutputPageIndexChanged(int value)
        {
            RefreshVisibleOutputs();
        }
        #endregion partial methods
    }
}
