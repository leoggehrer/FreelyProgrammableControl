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
        private readonly ExecutionUnit _executionUnit = new(20, 20);
        private const int DefaultCycleMs = 100;

        [ObservableProperty]
        private string _sourceCode = string.Empty;

        [ObservableProperty]
        private string _parseOutput = string.Empty;

        [ObservableProperty]
        private string _statusText = "Bereit";

        [ObservableProperty]
        private string _selectedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "newProgram.fpc");

        [ObservableProperty]
        private double _cycleTime = DefaultCycleMs;

        [ObservableProperty]
        private bool _isRunning = false;

        [ObservableProperty]
        private ObservableCollection<InputItemViewModel> _inputs = new();

        [ObservableProperty]
        private ObservableCollection<OutputItemViewModel> _outputs = new();

        [ObservableProperty]
        private ObservableCollection<CounterItemViewModel> _counters = new();

        public MainWindowViewModel()
        {
            InitializeExecutionUnit();
            UpdateInputOutputCounterViews();
        }

        private void InitializeExecutionUnit()
        {
            _executionUnit.CycleTimeMs = (int)CycleTime;
            _executionUnit.Inputs.Attach((s, e) => UpdateInputs());
            _executionUnit.Outputs.Attach((s, e) => UpdateOutputs());
            _executionUnit.Counters.Attach((s, e) => UpdateCounters());

            // Beispiel: Blinker auf Input 0
            _executionUnit.Inputs[0] = new Blinker(new TimeSpan(0, 0, 0, 0, 1000)) { Label = "Blinker 0" };
        }

        private void UpdateInputOutputCounterViews()
        {
            Inputs.Clear();
            for (int i = 0; i < _executionUnit.Inputs.Length; i++)
            {
                var inputVM = new InputItemViewModel
                {
                    Index = i,
                    Label = _executionUnit.Inputs[i].Label,
                    Value = _executionUnit.Inputs[i].Value,
                    IsModifiable = _executionUnit.Inputs[i].Modifiable,
                };
                inputVM.SetValueChangedCallback((idx, val) => OnInputValueChanged(idx, val));
                Inputs.Add(inputVM);
            }

            Outputs.Clear();
            for (int i = 0; i < _executionUnit.Outputs.Length; i++)
            {
                Outputs.Add(new OutputItemViewModel
                {
                    Index = i,
                    Label = _executionUnit.Outputs[i].Label,
                    Value = _executionUnit.Outputs[i].Value,
                });
            }

            Counters.Clear();
            for (int i = 0; i < _executionUnit.Counters.Length; i++)
            {
                Counters.Add(new CounterItemViewModel
                {
                    Index = i,
                    Value = _executionUnit.Counters.GetValue(i),
                });
            }
        }

        private void UpdateInputs()
        {
            foreach (var input in Inputs)
            {
                input.Value = _executionUnit.Inputs[input.Index].Value;
            }
        }

        private void UpdateOutputs()
        {
            foreach (var output in Outputs)
            {
                output.Value = _executionUnit.Outputs[output.Index].Value;
            }
        }

        private void UpdateCounters()
        {
            foreach (var counter in Counters)
            {
                counter.Value = _executionUnit.Counters.GetValue(counter.Index);
            }
        }

        private void OnInputValueChanged(int index, bool newValue)
        {
            if (index >= 0 && index < _executionUnit.Inputs.Length)
            {
                if (_executionUnit.Inputs[index] is Switch sw)
                {
                    // Toggle den Switch basierend auf dem gewünschten Wert
                    if (sw.Value != newValue)
                    {
                        sw.Toggle();
                    }
                }
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

            _executionUnit.CycleTimeMs = (int)CycleTime;
            var lines = SourceCode.Split(Environment.NewLine);

            _executionUnit.LoadSource(lines);
            if (_executionUnit.HasParseError)
            {
                StatusText = $"Parse-Fehler: {_executionUnit.ParseErrorMessage}";
                return;
            }

            _executionUnit.Start();
            IsRunning = true;
            StatusText = $"Läuft | Zyklus {_executionUnit.CycleTimeMs} ms";
        }

        [RelayCommand]
        private void Stop()
        {
            if (IsRunning)
            {
                _executionUnit.Stop();
                IsRunning = false;
                StatusText = "Gestoppt";
            }
        }

        [RelayCommand]
        private void ToggleInput(int index)
        {
            if (index >= 0 && index < _executionUnit.Inputs.Length)
            {
                if (_executionUnit.Inputs[index] is Switch sw)
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
                _executionUnit.Stop();
            }
        }

        partial void OnCycleTimeChanged(double oldValue, double newValue)
        {
            _executionUnit.CycleTimeMs = (int)Math.Max(1, newValue);
        }
    }
}
