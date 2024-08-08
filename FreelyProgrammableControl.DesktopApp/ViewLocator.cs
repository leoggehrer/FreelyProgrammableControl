using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FreelyProgrammableControl.DesktopApp.ViewModels;
using System;

namespace FreelyProgrammableControl.DesktopApp
{
    /// <summary>
    /// A class that implements the <see cref="IDataTemplate"/> interface to provide a mechanism for
    /// dynamically creating views based on their corresponding view models.
    /// </summary>
    /// <remarks>
    /// The <see cref="ViewLocator"/> class inspects the type of the provided data and attempts to
    /// instantiate a corresponding view by replacing "ViewModel" in the type name with "View".
    /// If the view cannot be found, it returns a <see cref="TextBlock"/> indicating that the view
    /// was not found.
    /// </remarks>
    public class ViewLocator : IDataTemplate
    {

        /// <summary>
        /// Builds a <see cref="Control"/> instance based on the provided data object.
        /// </summary>
        /// <param name="data">An optional data object used to determine the type of control to create. If null, the method returns null.</param>
        /// <returns>
        /// A <see cref="Control"/> instance corresponding to the type derived from the data object's type,
        /// or a <see cref="TextBlock"/> displaying a "Not Found" message if the type cannot be resolved.
        /// Returns null if the input data is null.
        /// </returns>
        public Control? Build(object? data)
        {
            if (data is null)
                return null;

            var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
            var type = Type.GetType(name);

            if (type != null)
            {
                var control = (Control)Activator.CreateInstance(type)!;
                control.DataContext = data;
                return control;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        /// <summary>
        /// Determines whether the specified data object is of type <see cref="ViewModelBase"/>.
        /// </summary>
        /// <param name="data">The data object to be checked.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="data"/> is of type <see cref="ViewModelBase"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
