using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreelyProgrammableControl.Logic.Execution;
using System;
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
    public partial class MainWindowViewModel : ViewModelBase
    {
        private IStorageProvider? storageProvider;
        private Window? ownerWindow;
        private bool isInitialized;
        private string? selectedFile;
        private string saveUserinput = string.Empty;

        private readonly ExecutionUnit executionUnit = new(20, 20);
        public ObservableCollection<InputDeviceViewModel> Inputs { get; } = new();
        public ObservableCollection<OutputDeviceViewModel> Outputs { get; } = new();

        [ObservableProperty]
        private string sourceText = string.Empty;

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

        public MainWindowViewModel()
        {
//            executionUnit.Inputs[0] = new Blinker(new TimeSpan(0, 0, 0, 0, 1000)) { Label = "Flasher 0" };
            selectedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "newProgram.fpc");
            StatusText = selectedFile;

            if (File.Exists(selectedFile))
            {
                var lines = File.ReadAllLines(selectedFile);

                SourceText = lines.Aggregate((a, b) => $"{a}{Environment.NewLine}{b}");
                executionUnit.LoadSource(lines);
            }

            executionUnit.Attach(UpdateExecutionState!);
            executionUnit.Inputs.Attach(OnUpdateInputs!);
            executionUnit.Outputs.Attach(OnUpdateOutputs!);

            CreateInputItems();
            CreateOutputItems();
        }

        public void Initialize(IStorageProvider? provider, Window owner)
        {
            if (isInitialized)
            {
                return;
            }

            storageProvider = provider;
            ownerWindow = owner;
            isInitialized = true;
        }

        [RelayCommand]
        private void New()
        {
            SourceText = string.Empty;
            selectedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "newProgram.fpc");
            StatusText = selectedFile;
        }

        [RelayCommand]
        private async Task OpenAsync()
        {
            if (storageProvider is null)
            {
                return;
            }

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

        [RelayCommand]
        private void Save()
        {
            if (selectedFile != null)
            {
                File.WriteAllText(selectedFile, SourceText ?? string.Empty);
            }
        }

        [RelayCommand]
        private async Task SaveAsAsync()
        {
            if (storageProvider is null)
            {
                return;
            }

            if (storageProvider.CanSave)
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
                    try
                    {
                        string content = SourceText ?? string.Empty;
                        await File.WriteAllTextAsync(result.Path.LocalPath, content);
                    }
                    catch (IOException ex)
                    {
                        if (ownerWindow != null)
                        {
                            var errorDialog = new Window
                            {
                                Width = 300,
                                Height = 200,
                                Content = new TextBlock
                                {
                                    Text = $"Error saving file: {ex.Message}",
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                                }
                            };
                            await errorDialog.ShowDialog(ownerWindow);
                        }
                    }
                }
            }
            else if (ownerWindow != null)
            {
                var errorDialog = new Window
                {
                    Width = 300,
                    Height = 200,
                    Content = new TextBlock
                    {
                        Text = "Saving is not supported on this platform.",
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    }
                };
                await errorDialog.ShowDialog(ownerWindow);
            }
        }

        [RelayCommand]
        private void Exit()
        {
            ownerWindow?.Close();
        }

        [RelayCommand]
        private void Undo()
        {
        }

        [RelayCommand]
        private void Redo()
        {
        }

        [RelayCommand]
        private void Copy()
        {
        }

        [RelayCommand]
        private void Paste()
        {
        }

        [RelayCommand]
        private void Cut()
        {
        }

        [RelayCommand(CanExecute = nameof(CanStart))]
        private void Start()
        {
            if (executionUnit.IsRunning == false && string.IsNullOrWhiteSpace(SourceText) == false)
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
            }
        }

        private bool CanStart()
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
            if (string.IsNullOrWhiteSpace(SourceText) == false)
            {
                ParseAndView(SourceText.Split(Environment.NewLine));
            }
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
        private void About()
        {
        }

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

        private void UpdateRunState()
        {
            IsSourceReadOnly = executionUnit.IsRunning;
            IsDebugEnabled = !executionUnit.IsRunning;

            ParseCommand.NotifyCanExecuteChanged();
            StartCommand.NotifyCanExecuteChanged();
            StopCommand.NotifyCanExecuteChanged();
            StepCommand.NotifyCanExecuteChanged();
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
    }
}
