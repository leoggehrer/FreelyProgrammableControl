
namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents a subject that can have observers attached to it.
    /// </summary>
    public interface ISubject
    {
        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>
        /// The total number of elements contained in the collection.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Attaches an observer to the event, allowing it to be notified when the event is raised.
        /// </summary>
        /// <param name="observer">The event handler that will be notified when the event occurs.</param>
        /// <remarks>
        /// This method allows multiple observers to be attached. Each observer will be invoked whenever the event is triggered.
        /// </remarks>
        void Attach(EventHandler observer);
        /// <summary>
        /// Detaches an observer from the event.
        /// </summary>
        /// <param name="observer">The event handler to be removed from the event subscribers.</param>
        /// <remarks>
        /// This method allows an observer to unsubscribe from the event notifications.
        /// If the specified observer is not currently attached, this method will have no effect.
        /// </remarks>
        void Detach(EventHandler observer);
    }
}