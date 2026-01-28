using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreelyProgrammableControl.Logic;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// Represents the view model for the main window of the application.
    /// </summary>
    /// <remarks>
    /// ViewModel für das Hauptfenster der FreelyProgrammableControl Desktop-App.
    /// Verwaltet die ExecutionUnit, Programm-Verwaltung, und UI-Status.
    /// </remarks>
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly ExecutionUnit executionUnit = new(20, 20);
        private const int DefaultCycleMs = 100;

        [ObservableProperty]
        private string sourceCode = string.Empty;

        [ObservableProperty]
        private string parseOutput = string.Empty;

        [ObservableProperty]
        private string statusText = "Bereit";

        [ObservableProperty]
        private string selectedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "newProgram.fpc");

        [ObservableProperty]
        private double cycleTime = DefaultCycleMs;

        [ObservableProperty]
        private bool isRunning = false;

        [ObservableProperty]
        private ObservableCollection<InputItemViewModel> inputs = new();

        [ObservableProperty]
        private ObservableCollection<OutputItemViewModel> outputs = new();

        [ObservableProperty]
        private ObservableCollection<CounterItemViewModel> counters = new();

        public MainWindowViewModel()
        {
            InitializeExecutionUnit();
            UpdateInputOutputCounterViews();
        }

        private void InitializeExecutionUnit()
        {
            executionUnit.CycleTimeMs = (int)CycleTime;
            executionUnit.Inputs.Attach((s, e) => UpdateInputs());
            executionUnit.Outputs.Attach((s, e) => UpdateOutputs());
            executionUnit.Counters.Attach((s, e) => UpdateCounters());

            // Beispiel: Blinker auf Input 0
            executionUnit.Inputs[0] = new Blinker(new TimeSpan(0, 0, 0, 0, 1000)) { Label = "Blinker 0" };
        }

        private void UpdateInputOutputCounterViews()
        {
            Inputs.Clear();
            for (int i = 0; i < executionUnit.Inputs.Length; i++)
            {
                Inputs.Add(new InputItemViewModel
                {
                    Index = i,
                    Label = executionUnit.Inputs[i].Label,
                    Value = executionUnit.Inputs[i].Value,
                    IsModifiable = executionUnit.Inputs[i].Modifiable,
                });
            }

            Outputs.Clear();
            for (int i = 0; i < executionUnit.Outputs.Length; i++)
            {
                Outputs.Add(new OutputItemViewModel
                {
                    Index = i,
                    Label = executionUnit.Outputs[i].Label,
                    Value = executionUnit.Outputs[i].Value,
                });
            }

            Counters.Clear();
            for (int i = 0; i < executionUnit.Counters.Length; i++)
            {
                Counters.Add(new CounterItemViewModel
                {
                    Index = i,
                    Value = executionUnit.Counters.GetValue(i),
                });
            }
        }

        private void UpdateInputs()
        {
            foreach (var input in Inputs)
            {
                input.Value = executionUnit.Inputs[input.Index].Value;
            }
        }

        private void UpdateOutputs()
        {
            foreach (var output in Outputs)
            {
                output.Value = executionUnit.Outputs[output.Index].Value;
            }
        }

        private void UpdateCounters()
        {
            foreach (var counter in Counters)
            {
                counter.Value = executionUnit.Counters.GetValue(counter.Index);
            }
        }

        [RelayCommand]
        private void New()
        {
            SourceCode = string.Empty;
            SelectedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "newProgram.fpc");
            StatusText = SelectedFile;
        }

        [RelayCommand]
        private void Open()
        {
            if (!string.IsNullOrEmpty(SelectedFile))
            {
                try
                {
                    SourceCode = File.ReadAllText(SelectedFile);
                    StatusText = SelectedFile;
                }
                catch (Exception ex)
                {
                    StatusText = $"Fehler beim Öffnen: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        private void Save()
        {
            if (!string.IsNullOrEmpty(SelectedFile))
            {
                try
                {
                    File.WriteAllText(SelectedFile, SourceCode);
                    StatusText = $"Gespeichert: {SelectedFile}";
                }
                catch (Exception ex)
                {
                    StatusText = $"Fehler beim Speichern: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        private void Parse()
        {
            if (string.IsNullOrEmpty(SourceCode))
            {
                StatusText = "Quelltextfeld ist leer";
                return;
            }

            var lines = SourceCode.Split(Environment.NewLine);
            var parsedLines = ExecutionUnit.Parse(lines);
            var sb = new StringBuilder();

            int errorCount = parsedLines.Count(pl => pl.HasError);
            sb.AppendLine($"Text hat {errorCount} Fehler");
            sb.AppendLine();

            foreach (var pl in parsedLines)
            {
                sb.AppendLine($"{pl.LineNumber:d4}: {pl.Source,-50} {(pl.HasError ? "Error" : ""),-8} {pl.ErrorMessage}");
            }

            ParseOutput = sb.ToString();
            StatusText = errorCount == 0 ? "Parse OK" : $"{errorCount} Parse-Fehler";
        }

        [RelayCommand]
        private void Start()
        {
            if (IsRunning || string.IsNullOrEmpty(SourceCode))
            {
                return;
            }

            executionUnit.CycleTimeMs = (int)CycleTime;
            var lines = SourceCode.Split(Environment.NewLine);

            executionUnit.LoadSource(lines);
            if (executionUnit.HasParseError)
            {
                StatusText = $"Parse-Fehler: {executionUnit.ParseErrorMessage}";
                return;
            }

            executionUnit.Start();
            IsRunning = true;
            StatusText = $"Läuft | Zyklus {executionUnit.CycleTimeMs} ms";
        }

        [RelayCommand]
        private void Stop()
        {
            if (IsRunning)
            {
                executionUnit.Stop();
                IsRunning = false;
                StatusText = "Gestoppt";
            }
        }

        [RelayCommand]
        private void ToggleInput(int index)
        {
            if (index >= 0 && index < executionUnit.Inputs.Length)
            {
                if (executionUnit.Inputs[index] is Switch sw)
                {
                    sw.Toggle();
                    if (index < Inputs.Count)
                    {
                        Inputs[index].Value = sw.Value;
                    }
                }
            }
        }

        [RelayCommand]
        private void Exit()
        {
            if (IsRunning)
            {
                executionUnit.Stop();
            }
        }

        partial void OnCycleTimeChanged(double oldValue, double newValue)
        {
            executionUnit.CycleTimeMs = (int)Math.Max(1, newValue);
        }
    }

    public partial class InputItemViewModel : ObservableObject
    {
        [ObservableProperty]
        public int index;

        [ObservableProperty]
        public string label = string.Empty;

        [ObservableProperty]
        public bool value;

        [ObservableProperty]
        public bool isModifiable;
    }

    public partial class OutputItemViewModel : ObservableObject
    {
        [ObservableProperty]
        public int index;

        [ObservableProperty]
        public string label = string.Empty;

        [ObservableProperty]
        public bool value;
    }

    public partial class CounterItemViewModel : ObservableObject
    {
        [ObservableProperty]
        public int index;

        [ObservableProperty]
        public int value;
    }
}
