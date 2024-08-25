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
    /// <summary>
    /// Represents the main window of the application, providing a user interface for
    /// loading, saving, and executing programs, as well as managing inputs and outputs.
    /// </summary>
    /// <remarks>
    /// This class is responsible for initializing the application, handling user
    /// interactions, and managing the execution unit that processes the loaded program.
    /// </remarks>
    public partial class MainWindow : Window
    {
        private string? selectedFile;
        private readonly ExecutionUnit executionUnit = new(20, 20);

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets up the initial state of the MainWindow, enabling the Start button
        /// and disabling the Stop button. It also initializes the path for the selected file
        /// and updates the status text accordingly. Furthermore, it attaches event handlers
        /// for input and output updates and creates the necessary UI elements for input checkboxes
        /// and output radio buttons.
        /// </remarks>
        public MainWindow()
        {
            InitializeComponent();

            Start.IsEnabled = true;
            Stop.IsEnabled = false;

            executionUnit.Inputs[0] = new Blinker(new TimeSpan(0, 0, 0, 0, 1000)) { Label = "Blinker 0" };
            selectedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "newProgram.fpc");
            Source.IsReadOnly = false;
            Status.Text = selectedFile;

            executionUnit.Inputs.Attach(OnUpdateInputs!);
            executionUnit.Outputs.Attach(OnUpdateOutputs!);

            CreateInputCheckBoxes();
            CreateOutputRadioButtons();
        }
        /// <summary>
        /// Handles the click event for the "New" action.
        /// This method clears the current source and sets the path for a new program file.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The RoutedEventArgs that contains the event data.</param>
        private void OnNewClick(object sender, RoutedEventArgs e)
        {
            // Code fuer "Neu" Aktion
            Source.Clear();
            selectedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "newProgram.fpc");
            Status.Text = selectedFile;
        }
        /// <summary>
        /// Handles the click event for the "Open" action.
        /// This method allows the user to pick a file from the storage provider.
        /// </summary>
        /// <param name="sender">The source of the event, typically the control that was clicked.</param>
        /// <param name="e">The event data associated with the click event.</param>
        /// <remarks>
        /// This method uses an asynchronous file picker to allow the user to select a single file.
        /// It supports filtering for all files and program files with specific extensions.
        /// If a file is selected, it reads the contents of the file and updates the relevant UI elements.
        /// </remarks>
        private async void OnOpenClick(object sender, RoutedEventArgs e)
        {
            // Code fuer "Open" Aktion
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

        /// <summary>
        /// Handles the click event for the save button.
        /// Saves the contents of the Source text box to the selected file.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        /// <remarks>
        /// If no file is selected, the method does nothing.
        /// </remarks>
        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            // Code fuer "Speichern" Aktion
            if (selectedFile != null)
            {
                File.WriteAllText(selectedFile, Source.Text);
            }
        }

        /// <summary>
        /// Handles the "Save As" action when the corresponding UI element is clicked.
        /// </summary>
        /// <param name="sender">The source of the event, typically the UI element that was clicked.</param>
        /// <param name="e">The event data associated with the click event.</param>
        /// <remarks>
        /// This method initiates a file save dialog, allowing the user to specify the location and name for saving a file.
        /// It supports saving files with specific extensions and handles potential errors during the file writing process.
        /// If the save operation is not supported on the current platform, an error dialog is displayed.
        /// </remarks>
        /// <exception cref="IOException">Thrown when there is an error during the file writing process.</exception>
        private async void OnSaveAsClick(object sender, RoutedEventArgs e)
        {
            // Code fuer "Speichern unter" Aktion
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
                        Text = "Der Speicherdienst wird auf dieser Plattform nicht unterstuetzt.",
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    }
                };
                await errorDialog.ShowDialog(this);
            }
        }

        /// <summary>
        /// Handles the click event for the exit button.
        /// Closes the application when the exit button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event, typically the exit button.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            // Anwendung beenden
            Close();
        }

        /// <summary>
        /// Handles the click event for the "Undo" action.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void OnUndoClick(object sender, RoutedEventArgs e)
        {
            // Code fuer "Rueckguengig" Aktion
        }

        /// <summary>
        /// Handles the click event for the "Redo" action.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data containing the event information.</param>
        private void OnRedoClick(object sender, RoutedEventArgs e)
        {
            // Code fuer "Wiederholen" Aktion
        }

        /// <summary>
        /// Handles the click event for the copy action.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            // Code fuer "Kopieren" Aktion
        }

        /// <summary>
        /// Handles the click event for the paste action.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void OnPasteClick(object sender, RoutedEventArgs e)
        {
            // Code fuer "Einfuegen" Aktion
        }

        /// <summary>
        /// Handles the click event for the cut action.
        /// This method is triggered when the user selects the cut option.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data containing the event information.</param>
        private void OnCutClick(object sender, RoutedEventArgs e)
        {
            // Code fuer "Ausschneiden" Aktion
        }

        /// <summary>
        /// Handles the click event for the "Start" button.
        /// Initiates the execution process if the execution unit is not currently running
        /// and the source text is provided.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data associated with the click event.</param>
        /// <remarks>
        /// This method checks if the execution unit is not running and that the source text is not empty.
        /// If both conditions are met, it splits the source text into lines, loads them into the execution unit,
        /// and starts the execution. It then updates the enabled state of the Start and Stop buttons
        /// based on the execution unit's running status.
        /// </remarks>
        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            // Code fuer "Start" Aktion

            if (executionUnit.IsRunning == false && Source.Text != default)
            {
                int errors = 0;
                var lines = Source.Text!.Split(Environment.NewLine);

                errors = ParseAndView(lines);

                if (errors == 0)
                {
                    executionUnit.LoadSource(lines);
                    executionUnit.Start();

                    Start.IsEnabled = executionUnit.IsRunning == false;
                    Stop.IsEnabled = executionUnit.IsRunning;
                    Source.IsReadOnly = true;
                }
            }
        }
        /// <summary>
        /// Handles the click event for the "Stop" button.
        /// </summary>
        /// <param name="sender">The source of the event, typically the "Stop" button.</param>
        /// <param name="e">The event data associated with the click event.</param>
        /// <remarks>
        /// This method checks if the execution unit is currently running. If it is, the method stops the execution unit.
        /// It then updates the enabled state of the "Start" and "Stop" buttons based on whether the execution unit is running.
        /// </remarks>
        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            // Code fuer "Stop" Aktion
            if (executionUnit.IsRunning)
            {
                executionUnit.Stop();
            }
            Start.IsEnabled = executionUnit.IsRunning == false;
            Stop.IsEnabled = executionUnit.IsRunning;
            Source.IsReadOnly = false;
        }
        /// <summary>
        /// Handles the click event for the "Parse" action.
        /// </summary>
        /// <param name="sender">The source of the event, typically the UI element that was clicked.</param>
        /// <param name="e">The event data associated with the click event.</param>
        /// <remarks>
        /// This method retrieves text from a source control, splits it into lines, and parses each line.
        /// It generates a formatted output that includes line numbers, the original text, error status,
        /// and error messages if any. The output is then displayed in an output control.
        /// </remarks>
        private void OnParseClick(object sender, RoutedEventArgs e)
        {
            // Code fuer "Parse" Aktion
            if (Source.Text != default)
            {
                // Split the text into lines and parse them
                ParseAndView(Source.Text!.Split(Environment.NewLine));
            }
        }

        /// <summary>
        /// Parses the provided lines and updates the view with the parsed results.
        /// </summary>
        /// <param name="lines">An array of strings representing the lines to be parsed.</param>
        /// <returns>The number of errors.</returns>
        /// <remarks>
        /// This method uses the <see cref="ExecutionUnit.Parse"/> method to parse the provided lines.
        /// It then formats the parsed results, including line numbers, original text, error status,
        /// and error messages if any. The formatted output is displayed in the output control.
        /// </remarks>
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

            Output.Clear();
            Output.Text = string.Join(Environment.NewLine, parsedText);
            return errorCount;
        }

        /// <summary>
        /// Handles the click event for the "About" action.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            // Code fuer "ueber" Aktion
        }

        /// <summary>
        /// Handles the update of input values when an event is triggered.
        /// This method is invoked asynchronously on the UI thread to update the state of checkboxes
        /// based on the values provided by the <see cref="IInputs"/> interface.
        /// </summary>
        /// <param name="sender">The source of the event, typically the object that raised the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
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
        /// <summary>
        /// Handles the update of output values from the specified sender.
        /// This method is invoked asynchronously on the UI thread and updates the state
        /// of radio buttons based on the provided outputs.
        /// </summary>
        /// <param name="sender">The source of the event that triggered this method.</param>
        /// <param name="e">The event data associated with the event.</param>
        /// <remarks>
        /// This method checks if the sender is of type <see cref="IOutputs"/> and updates
        /// the corresponding <see cref="RadioButton"/> controls in the UI based on the values
        /// provided by the <see cref="IOutputs"/> instance.
        /// </remarks>
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
        /// <summary>
        /// Creates and adds checkbox controls for each input in the execution unit.
        /// </summary>
        /// <remarks>
        /// This method iterates through the inputs of the execution unit,
        /// creating a checkbox for each input. Each checkbox is configured
        /// with its label, enabled state, margin, and a tag corresponding to
        /// its index. The method also subscribes to the <see cref="CheckBox_Checked"/>
        /// event for handling changes in the checkbox state.
        /// </remarks>
        private void CreateInputCheckBoxes()
        {
            for (int i = 0; i < executionUnit.Inputs.Length; i++)
            {
                var checkBox = new CheckBox
                {
                    IsEnabled = executionUnit.Inputs[i].Modifiable,
                    Content = $"{executionUnit.Inputs[i].Label}",
                    Margin = new Thickness(0),
                    Tag = i,
                };

                checkBox.IsCheckedChanged += CheckBox_Checked!;
                Inputs.Children.Add(checkBox);
            }
        }

        /// <summary>
        /// Creates and adds radio buttons to the output container for each output defined in the execution unit.
        /// Each radio button is disabled and is grouped by its index.
        /// </summary>
        /// <remarks>
        /// The method iterates through the outputs of the execution unit, creating a radio button for each output.
        /// The content of the radio button is set to the label of the corresponding output.
        /// The radio buttons are added to the <see cref="Outputs"/> container.
        /// </remarks>
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

                // Optional: Event-Handler fuer Checked-Ereignis
                Outputs.Children.Add(radioButton);
            }
        }

        /// <summary>
        /// Handles the Checked event for a CheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event, typically the CheckBox that was checked or unchecked.</param>
        /// <param name="e">The RoutedEventArgs that contains the event data.</param>
        /// <remarks>
        /// This method checks if the sender is a CheckBox and retrieves its index from the Tag property.
        /// It then checks if the corresponding input in the execution unit is a Switch.
        /// If the CheckBox's checked state does not match the Switch's value, it toggles the Switch.
        /// </remarks>
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