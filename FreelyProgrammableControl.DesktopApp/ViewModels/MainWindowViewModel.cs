using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreelyProgrammableControl.Logic.Execution;
using FreelyProgrammableControl.DesktopApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// Represents the view model for the main window of the application.
    /// </summary>
    /// <remarks>
    /// This class inherits from <see cref="ViewModelBase"/> and provides data and
    /// functionality for the main window's user interface.
    /// </remarks>
    public partial class MainWindowViewModel : ViewModelBase, IDisposable
    {
        #region fields
        private Window? ownerWindow;
        private IStorageProvider? storageProvider;
        private IClipboard? clipboard;
        private ApiService? apiService;
        private bool isInitialized;
        private string? selectedFile;
        private readonly Stack<string> undoStack = new();
        private readonly Stack<string> redoStack = new();
        private string lastSourceText = string.Empty;
        private string saveUserinput = string.Empty;
        private readonly ExecutionUnit executionUnit = new(20, 20);
        #endregion fields

        #region observable properties
        public ObservableCollection<InputDeviceViewModel> Inputs { get; } = new();
        public ObservableCollection<OutputDeviceViewModel> Outputs { get; } = new();

        [ObservableProperty]
        private string sourceText = string.Empty;

        partial void OnSourceTextChanged(string value)
        {
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
        }

        [ObservableProperty]
        private string outputText = string.Empty;

        [ObservableProperty]
        private string executionState = string.Empty;

        [ObservableProperty]
        private string statusText = string.Empty;

        [ObservableProperty]
        private bool isSourceReadOnly;

        [ObservableProperty]
        private bool isDebugEnabled = true;

        [ObservableProperty]
        private bool debugEnabled;

        [ObservableProperty]
        private string debugButtonText = "Debug: OFF";
        #endregion observable properties

        #region properties
        public bool IsRunning => executionUnit.IsRunning;
        public bool HasParseError => executionUnit.HasParseError;
        public string? ParseErrorMessage => executionUnit.ParseErrorMessage;
        public string[] Source => executionUnit.Source;
        public string State => executionUnit.State;
        #endregion properties

        #region constructor and initialization
        public MainWindowViewModel()
        {
            //            executionUnit.Inputs[0] = new Blinker(new TimeSpan(0, 0, 0, 0, 1000)) { Label = "Flasher 0" };
            selectedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "newProgram.fpc");
            StatusText = selectedFile;

            if (File.Exists(selectedFile))
            {
                var lines = File.ReadAllLines(selectedFile);

                SourceText = lines.Length > 0 ? lines.Aggregate((a, b) => $"{a}{Environment.NewLine}{b}") : string.Empty;
                executionUnit.LoadSource(lines);
            }

            executionUnit.Attach(UpdateExecutionState!);
            executionUnit.Inputs.Attach(OnUpdateInputs!);
            executionUnit.Outputs.Attach(OnUpdateOutputs!);

            CreateInputItems();
            CreateOutputItems();

            // Start API Service
            StartApiService();
        }

        private async void StartApiService()
        {
            try
            {
                var settings = ConfigurationHelper.GetSettings();

                apiService = new ApiService(this, settings.Api.Port);
                await apiService.StartAsync();
                StatusText = $"{selectedFile} - API läuft auf Port {apiService.Port}";
            }
            catch (Exception ex)
            {
                StatusText = $"API-Server konnte nicht gestartet werden: {ex.Message}";
            }
        }

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
        }
        #endregion constructor and initialization

        #region commands
        [RelayCommand]
        private void New()
        {
            SourceText = string.Empty;
            selectedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "newProgram.fpc");
            StatusText = selectedFile;
        }

        [RelayCommand(CanExecute = nameof(CanOpen))]
        private async Task OpenAsync()
        {
            if (storageProvider is null)
            {
                await ShowErrorDialogAsync("Fehler", "Dateisystem nicht verfügbar.");
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
                await ShowErrorDialogAsync("Fehler beim Öffnen", $"Die Datei konnte nicht geöffnet werden:\n{ex.Message}");
            }
        }
        private bool CanOpen()
        {
            return executionUnit.IsRunning == false;
        }

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
                    await ShowErrorDialogAsync("Fehler beim Speichern", $"Die Datei konnte nicht gespeichert werden:\n{ex.Message}");
                }
            }
        }

        private bool CanSave()
        {
            return executionUnit.IsRunning == false;
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsAsync()
        {
            if (storageProvider is null)
            {
                await ShowErrorDialogAsync("Fehler", "Dateisystem nicht verfügbar.");
                return;
            }

            if (!storageProvider.CanSave)
            {
                await ShowErrorDialogAsync("Nicht unterstützt", "Speichern wird auf dieser Plattform nicht unterstützt.");
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
                await ShowErrorDialogAsync("Fehler beim Speichern", $"Die Datei konnte nicht gespeichert werden:\n{ex.Message}");
            }
        }

        [RelayCommand]
        private void Exit()
        {
            // Cleanup API Service
            apiService?.Dispose();
            apiService = null;

            ownerWindow?.Close();
        }

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

        [RelayCommand(CanExecute = nameof(CanCopyOrCut))]
        private async Task CopyAsync()
        {
            if (clipboard != null && !string.IsNullOrEmpty(SourceText))
            {
                await clipboard.SetTextAsync(SourceText);
            }
        }

        private bool CanCopyOrCut() => !string.IsNullOrEmpty(SourceText);

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

        [RelayCommand(CanExecute = nameof(CanCopyOrCut))]
        private async Task CutAsync()
        {
            if (clipboard != null && !string.IsNullOrEmpty(SourceText))
            {
                await clipboard.SetTextAsync(SourceText);
                SourceText = string.Empty;
            }
        }

        [RelayCommand(CanExecute = nameof(CanStart))]
        private async Task StartAsync()
        {
            if (executionUnit.IsRunning == false && string.IsNullOrWhiteSpace(SourceText) == false)
            {
                try
                {
                    var source = SourceText.Split(Environment.NewLine);
                    var errors = ParseAndView(source);

                    if (errors == 0)
                    {
                        executionUnit.LoadSource(source);
                        executionUnit.Start();

                        saveUserinput = SourceText;
                        SourceText = ExecutionUnit.PrepareSource(source)
                                                  .Select((i, l) => $"{l:d4}: {i}")
                                                  .Aggregate((a, b) => $"{a}{Environment.NewLine}{b}");

                        UpdateRunState();
                    }
                    else
                    {
                        await ShowErrorDialogAsync("Parse-Fehler", $"Das Programm enthält {errors} Fehler und kann nicht gestartet werden.");
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorDialogAsync("Fehler beim Start", $"Das Programm konnte nicht gestartet werden:\n{ex.Message}");
                }
            }
        }

        private bool CanStart()
        {
            return executionUnit.IsRunning == false;
        }

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

        [RelayCommand(CanExecute = nameof(CanParse))]
        private void Parse()
        {
            ParseAndView(SourceText.Split(Environment.NewLine));
        }

        private bool CanParse()
        {
            return executionUnit.IsRunning == false;
        }

        [RelayCommand(CanExecute = nameof(CanStep))]
        private void Step()
        {
            if (executionUnit.IsRunning)
            {
                executionUnit.Step();
            }
            UpdateRunState();
        }

        private bool CanStep()
        {
            return executionUnit.IsRunning && executionUnit.DebugEnabled;
        }

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
        #endregion commands

        private int ParseAndView(string[] lines)
        {
            var parsedLines = ExecutionUnit.Parse(lines);
            var parsedText = parsedLines.Select(pl =>
            {
                var result = $"{pl.LineNumber:d4}: {pl.Source,-50} {(pl.HasError ? "Error" : ""),-8} {pl.ErrorMessage}";

                return result;
            }).ToList();
            var errorCount = parsedLines.Count(pl => pl.HasError);

            parsedText.Insert(0, $"Text has {errorCount} Error(s)");
            parsedText.Insert(1, string.Empty);

            OutputText = string.Join(Environment.NewLine, parsedText);
            return errorCount;
        }

        public void UpdateRunState()
        {
            IsSourceReadOnly = executionUnit.IsRunning;
            IsDebugEnabled = !executionUnit.IsRunning;

            ParseCommand.NotifyCanExecuteChanged();
            StartCommand.NotifyCanExecuteChanged();
            StopCommand.NotifyCanExecuteChanged();
            StepCommand.NotifyCanExecuteChanged();
            UndoCommand?.NotifyCanExecuteChanged();
            RedoCommand?.NotifyCanExecuteChanged();
            CopyCommand?.NotifyCanExecuteChanged();
            PasteCommand?.NotifyCanExecuteChanged();
            CutCommand?.NotifyCanExecuteChanged();
        }

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

        private void UpdateExecutionState(object sender, EventArgs e)
        {
            ExecutionState = executionUnit.State;
        }

        partial void OnDebugEnabledChanged(bool value)
        {
            executionUnit.DebugEnabled = value;
            DebugButtonText = value ? "Debug: ON" : "Debug: OFF";
        }

        private void CreateInputItems()
        {
            Inputs.Clear();
            for (int i = 0; i < executionUnit.Inputs.Length; i++)
            {
                Inputs.Add(new InputDeviceViewModel(executionUnit.Inputs[i]));
            }
        }

        private void CreateOutputItems()
        {
            Outputs.Clear();
            for (int i = 0; i < executionUnit.Outputs.Length; i++)
            {
                Outputs.Add(new OutputDeviceViewModel(executionUnit.Outputs[i], i));
            }
        }

        /// <summary>
        /// Shows an error dialog to the user
        /// </summary>
        private async Task ShowErrorDialogAsync(string title, string message)
        {
            if (ownerWindow != null)
            {
                var errorDialog = new Window
                {
                    Title = title,
                    Width = 450,
                    Height = 200,
                    CanResize = false,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var okButton = new Button
                {
                    Content = "OK",
                    Width = 100,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };
                okButton.Click += (s, e) => errorDialog.Close();

                errorDialog.Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 15,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            FontSize = 14
                        },
                        okButton
                    }
                };

                await errorDialog.ShowDialog(ownerWindow);
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
    }
}
