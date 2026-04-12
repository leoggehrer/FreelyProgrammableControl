using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// Base class for all view models in the application.
    /// Provides property-change notification via <see cref="ObservableObject"/>
    /// and shared UI helpers used across multiple view models.
    /// </summary>
    public class ViewModelBase : ObservableObject
    {
        /// <summary>
        /// Shows a modal error dialog with an OK button.
        /// No-op when <paramref name="owner"/> is <c>null</c>.
        /// </summary>
        /// <param name="owner">The parent window that owns the dialog.</param>
        /// <param name="title">Dialog window title.</param>
        /// <param name="message">Error message shown to the user.</param>
        protected static async Task ShowErrorDialogAsync(Window? owner, string title, string message)
        {
            if (owner == null) return;

            var dialog = new Window
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
            okButton.Click += (s, e) => dialog.Close();

            dialog.Content = new StackPanel
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

            await dialog.ShowDialog(owner);
        }

        /// <summary>
        /// Shows a modal informational dialog with an OK button.
        /// No-op when <paramref name="owner"/> is <c>null</c>.
        /// </summary>
        /// <param name="owner">The parent window that owns the dialog.</param>
        /// <param name="title">Dialog window title.</param>
        /// <param name="message">Informational message shown to the user.</param>
        protected static async Task ShowInfoDialogAsync(Window? owner, string title, string message)
        {
            if (owner == null) return;

            var dialog = new Window
            {
                Title = title,
                Width = 450,
                Height = 180,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 100,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            okButton.Click += (s, e) => dialog.Close();

            dialog.Content = new StackPanel
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

            await dialog.ShowDialog(owner);
        }

        /// <summary>
        /// Shows a modal dialog that lets the user enter a new label text.
        /// Returns the entered text, or <c>null</c> if the user cancelled.
        /// </summary>
        /// <param name="owner">The parent window that owns the dialog.</param>
        /// <param name="currentLabel">The label text pre-filled in the input field.</param>
        /// <returns>The new label string, or <c>null</c> if the dialog was cancelled or the input was empty.</returns>
        protected static async Task<string?> ShowLabelEditDialogAsync(Window owner, string currentLabel)
        {
            var dialog = new Window
            {
                Title = "Label ändern",
                Width = 400,
                Height = 180,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var textBox = new TextBox
            {
                Text = currentLabel,
                Watermark = "Neuer Label",
                Margin = new Avalonia.Thickness(10)
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 100,
                Margin = new Avalonia.Thickness(5)
            };

            var cancelButton = new Button
            {
                Content = "Abbrechen",
                Width = 100,
                Margin = new Avalonia.Thickness(5)
            };

            bool? result = null;

            okButton.Click += (s, e) => { result = true; dialog.Close(); };
            cancelButton.Click += (s, e) => { result = false; dialog.Close(); };

            textBox.KeyDown += (s, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Enter)        { result = true;  dialog.Close(); }
                else if (e.Key == Avalonia.Input.Key.Escape)  { result = false; dialog.Close(); }
            };

            dialog.Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 15,
                Children =
                {
                    new TextBlock { Text = "Geben Sie einen neuen Label ein:", FontSize = 14 },
                    textBox,
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Children = { okButton, cancelButton }
                    }
                }
            };

            await dialog.ShowDialog(owner);

            return result == true && !string.IsNullOrWhiteSpace(textBox.Text)
                ? textBox.Text
                : null;
        }
    }
}
