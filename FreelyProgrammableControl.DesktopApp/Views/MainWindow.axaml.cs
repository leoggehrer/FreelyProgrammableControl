using System;
using Avalonia.Controls;
using Avalonia.Input;
using FreelyProgrammableControl.DesktopApp.ViewModels;

namespace FreelyProgrammableControl.DesktopApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.Initialize(StorageProvider, this);
            }
        }

        private void Output_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is Border border && border.DataContext is OutputDeviceViewModel outputViewModel)
            {
                outputViewModel.EditLabelCommand?.Execute(null);
            }
        }

        private void Input_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is Border border && border.DataContext is InputDeviceViewModel inputViewModel)
            {
                inputViewModel.EditLabelCommand?.Execute(null);
            }
        }

        private void SourceTextBox_DoubleTapped(object? sender, TappedEventArgs e)
        {
            // Nur wenn Debug-Modus aktiviert ist
            if (DataContext is not MainWindowViewModel viewModel || !viewModel.DebugEnabled)
            {
                return;
            }

            if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
            {
                var caretIndex = textBox.CaretIndex;
                var text = textBox.Text;

                // Finde Start der aktuellen Zeile
                int lineStart = caretIndex;
                while (lineStart > 0 && text[lineStart - 1] != '\n' && text[lineStart - 1] != '\r')
                {
                    lineStart--;
                }

                // Finde Ende der aktuellen Zeile
                int lineEnd = caretIndex;
                while (lineEnd < text.Length && text[lineEnd] != '\n' && text[lineEnd] != '\r')
                {
                    lineEnd++;
                }

                // Markiere die gesamte Zeile
                textBox.SelectionStart = lineStart;
                textBox.SelectionEnd = lineEnd;
            }
        }

        private void SourceTextBox_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            // Nur wenn Debug-Modus aktiviert ist, Mehrfachauswahl verhindern
            if (DataContext is not MainWindowViewModel viewModel || !viewModel.DebugEnabled)
            {
                return;
            }

            if (sender is TextBox textBox && textBox.SelectionStart >= 0 && textBox.SelectionEnd > textBox.SelectionStart)
            {
                var text = textBox.Text;
                if (string.IsNullOrEmpty(text))
                    return;

                var selectionStart = textBox.SelectionStart;
                var selectionEnd = textBox.SelectionEnd;

                // Prüfe ob die Auswahl mehrere Zeilen umfasst
                bool hasNewline = false;
                for (int i = selectionStart; i < selectionEnd && i < text.Length; i++)
                {
                    if (text[i] == '\n' || text[i] == '\r')
                    {
                        hasNewline = true;
                        break;
                    }
                }

                if (hasNewline)
                {
                    // Mehrere Zeilen ausgewählt - beschränke auf erste Zeile
                    int lineEnd = selectionStart;
                    while (lineEnd < text.Length && text[lineEnd] != '\n' && text[lineEnd] != '\r')
                    {
                        lineEnd++;
                    }

                    textBox.SelectionStart = selectionStart;
                    textBox.SelectionEnd = lineEnd;
                }
            }
        }
    }
}
