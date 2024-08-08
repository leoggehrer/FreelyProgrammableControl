using CommunityToolkit.Mvvm.ComponentModel;

namespace FreelyProgrammableControl.DesktopApp.ViewModels
{
    /// <summary>
    /// Represents the base class for view models in the application, providing
    /// functionality for property change notification through the
    /// <see cref="ObservableObject"/> base class.
    /// </summary>
    /// <remarks>
    /// This class serves as a foundation for all view models, allowing them
    /// to inherit common behavior for notifying the UI of property changes,
    /// which is essential for data binding in MVVM (Model-View-ViewModel)
    /// architecture.
    /// </remarks>
    public class ViewModelBase : ObservableObject
    {
    }
}
