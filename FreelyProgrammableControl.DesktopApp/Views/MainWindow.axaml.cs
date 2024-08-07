using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FreelyProgrammableControl.Logic;
using System;
using System.IO;
using System.Linq;

namespace FreelyProgrammableControl.DesktopApp.Views
{
    public partial class MainWindow : Window
    {
        private string? selectedFile;
        private readonly ExecutionUnit executionUnit = new(16, 16);

        public MainWindow()
        {
            InitializeComponent();

            Start.IsEnabled = true;
            Stop.IsEnabled = false;

            selectedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "newProgram.fpc");
            Status.Text = selectedFile;

            executionUnit.Inputs.Attach(OnUpdateInputs!);
            executionUnit.Outputs.Attach(OnUpdateOutputs!);

            CreateInputCheckBoxes();
            CreateOutputRadioButtons();
        }
        private void OnNewClick(object sender, RoutedEventArgs e)
        {
            // Code für "Neu" Aktion
            Source.Clear();
            selectedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "newProgram.fpc");
            Status.Text = selectedFile;
        }
        private async void OnOpenClick(object sender, RoutedEventArgs e)
        {
            // Code für "Öffnen" Aktion
            var storageProvider = StorageProvider;

            if (storageProvider != null)
            {
                var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    AllowMultiple = false, // Set to true if multiple files should be allowed
                    FileTypeFilter =
                    [
                        new FilePickerFileType("Alle Dateien") { Patterns = ["*"] },
                        new FilePickerFileType("Programmdateien") { Patterns = ["*.fpc"] }
                    ]
                });

                if (result.Count == 1)
                {
                    selectedFile = result[0].Path.LocalPath;

                    Source.Clear();
                    Source.Text = File.ReadAllText(selectedFile);
                    Status.Text = selectedFile;
                }
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            // Code für "Speichern" Aktion
            if (selectedFile != null)
            {
                File.WriteAllText(selectedFile, Source.Text);
            }
        }

        private async void OnSaveAsClick(object sender, RoutedEventArgs e)
        {
            // Code für "Speichern unter" Aktion
            var storageProvider = StorageProvider;

            if (storageProvider.CanSave)
            {
                var saveOptions = new FilePickerSaveOptions
                {
                    Title = "Speichern unter...",
                    FileTypeChoices =
                    [
                        new FilePickerFileType("Alle Dateien") { Patterns = ["*"] },
                        new FilePickerFileType("Programmdateien") { Patterns = ["*.fpc"] }
                    ],
                    SuggestedFileName = Path.GetFileName(selectedFile), // Setzt den Startdateinamen
                    SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(selectedFile!) // Setzt das Startverzeichnis
                };

                var result = await storageProvider.SaveFilePickerAsync(saveOptions);

                if (result != null)
                {
                    try
                    {
                        // Beispielinhalt speichern
                        string content = Source.Text ?? string.Empty;
                        await File.WriteAllTextAsync(result.Path.LocalPath, content);
                    }
                    catch (IOException ex)
                    {
                        var errorDialog = new Window
                        {
                            Width = 300,
                            Height = 200,
                            Content = new TextBlock
                            {
                                Text = $"Fehler beim Speichern der Datei: {ex.Message}",
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                            }
                        };
                        await errorDialog.ShowDialog(this);
                    }
                }
            }
            else
            {
                var errorDialog = new Window
                {
                    Width = 300,
                    Height = 200,
                    Content = new TextBlock
                    {
                        Text = "Der Speicherdienst wird auf dieser Plattform nicht unterstützt.",
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    }
                };
                await errorDialog.ShowDialog(this);
            }
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            // Anwendung beenden
            Close();
        }

        private void OnUndoClick(object sender, RoutedEventArgs e)
        {
            // Code für "Rückgängig" Aktion
        }

        private void OnRedoClick(object sender, RoutedEventArgs e)
        {
            // Code für "Wiederholen" Aktion
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            // Code für "Kopieren" Aktion
        }

        private void OnPasteClick(object sender, RoutedEventArgs e)
        {
            // Code für "Einfügen" Aktion
        }

        private void OnCutClick(object sender, RoutedEventArgs e)
        {
            // Code für "Ausschneiden" Aktion
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            // Code für "Start" Aktion
            if (executionUnit.IsRunning == false && Source.Text != default)
            {
                var lines = Source.Text!.Split(Environment.NewLine);

                executionUnit.LoadSource(lines);
                executionUnit.Start();
            }
            Start.IsEnabled = executionUnit.IsRunning == false;
            Stop.IsEnabled = executionUnit.IsRunning;
        }
        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            // Code für "Stop" Aktion
            if (executionUnit.IsRunning)
            {
                executionUnit.Stop();
            }
            Start.IsEnabled = executionUnit.IsRunning == false;
            Stop.IsEnabled = executionUnit.IsRunning;
        }
        private void OnParseClick(object sender, RoutedEventArgs e)
        {
            // Code für "Parse" Aktion
            var lines = Source.Text!.Split(Environment.NewLine);
            var parsedLines = ExecutionUnit.Parse(lines);
            var parseText = parsedLines.Select(pl =>
            {
                var result = $"{pl.LineNumber, -4}: {pl.Source, -100} {(pl.HasError ? pl.HasError : ""), -8} {pl.ErrorMessage}";

                return result;
            }).ToList();
            var errorCount = parsedLines.Count(pl => pl.HasError);

            parseText.Insert(0, $"Text has {errorCount} Error(s)");
            parseText.Insert(1, string.Empty);
           
            Output.Clear();
            Output.Text = string.Join(Environment.NewLine, parseText);
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            // Code für "Über" Aktion
        }

        private void OnUpdateInputs(object sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (sender is IInputs inputs)
                {
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        if (Inputs.Children[i] is CheckBox checkBox)
                        {
                            checkBox.IsChecked = inputs[i].Value;
                        }
                    }
                }
            });
        }
        private void OnUpdateOutputs(object sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (sender is IOutputs outputs)
                {
                    for (int i = 0; i < outputs.Length; i++)
                    {
                        if (Outputs.Children[i] is RadioButton radio)
                        {
                            radio.IsChecked = outputs[i].Value;
                        }
                    }
                }
            });
        }
        private void CreateInputCheckBoxes()
        {
            for (int i = 0; i < executionUnit.Inputs.Length; i++)
            {
                var checkBox = new CheckBox
                {
                    IsEnabled = true,
                    Content = $"{executionUnit.Inputs[i].Label}",
                    Margin = new Thickness(0),
                    Tag = i,
                };

                checkBox.IsCheckedChanged += CheckBox_Checked!;
                Inputs.Children.Add(checkBox);
            }
        }

        private void CreateOutputRadioButtons()
        {
            for (int i = 0; i < executionUnit.Outputs.Length; i++)
            {
                var radioButton = new RadioButton
                {
                    GroupName = i.ToString(),
                    IsEnabled = false,
                    Content = $"{executionUnit.Outputs[i].Label}",
                    Margin = new Thickness(0),
                    Tag = i,
                };

                // Optional: Event-Handler für Checked-Ereignis

                Outputs.Children.Add(radioButton);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                int idx = Convert.ToInt32(checkBox.Tag);

                if (executionUnit.Inputs[idx] is Switch sw)
                {
                    if (checkBox.IsChecked != sw.Value)
                    {
                        sw.Toggle();
                    }
                }
            }
        }
    }
}