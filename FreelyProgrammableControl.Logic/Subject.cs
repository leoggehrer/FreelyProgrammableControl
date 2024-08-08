namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents an abstract subject class that maintains a list of observers and provides methods to manage them.
    /// </summary>
    /// <remarks>
    /// This class implements the observer design pattern, allowing observers to be attached or detached and notified of changes.
    /// </remarks>
    public abstract class Subject : ISubject
    {
        #region fields
        private readonly List<EventHandler> observers = [];
        #endregion fields

        #region  properties
        /// <summary>
        /// Gets the number of observers currently registered.
        /// </summary>
        /// <value>
        /// The count of observers.
        /// </value>
        public int Count => observers.Count;
        #endregion properties

        #region constructors
        #endregion constructors

        #region methods
        /// <summary>
        /// Attaches an observer to the event notification system.
        /// </summary>
        /// <param name="observer">The event handler to be added as an observer.</param>
        /// <remarks>
        /// This method allows an observer to be registered so that it can receive notifications
        /// when the event occurs. Multiple observers can be attached.
        /// </remarks>
        public void Attach(EventHandler observer)
        {
            observers.Add(observer);
        }

        /// <summary>
        /// Removes the specified observer from the list of observers.
        /// </summary>
        /// <param name="observer">The <see cref="EventHandler"/> to be removed from the observer list.</param>
        /// <remarks>
        /// This method will not throw an exception if the observer is not found in the list.
        /// </remarks>
        public void Detach(EventHandler observer)
        {
            observers.Remove(observer);
        }

        /// <summary>
        /// Notifies all registered observers by invoking their event handlers.
        /// </summary>
        /// <remarks>
        /// This method iterates through the list of observers and calls each observer's
        /// event handler, passing the current instance and an empty <see cref="EventArgs"/>
        /// as parameters. This is typically used in the observer pattern to inform
        /// subscribers of an event or state change.
        /// </remarks>
        protected void Notify()
        {
            foreach (EventHandler observer in observers)
            {
                observer.Invoke(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// Asynchronously notifies by running the <see cref="Notify"/> method on a separate thread.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected Task NotifyAsync()
        {
            return Task.Run(() => Notify());
        }
        #endregion methods
    }
}
