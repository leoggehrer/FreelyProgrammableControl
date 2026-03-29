namespace FreelyProgrammableControl.Logic.Contracts
{
    /// <summary>
    /// Defines observer registration for subject implementations.
    /// </summary>
    public interface ISubject
    {
        /// <summary>
        /// Gets the number of currently attached observers.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Registers an observer callback.
        /// </summary>
        /// <param name="observer">Observer to register.</param>
        void Attach(EventHandler observer);

        /// <summary>
        /// Unregisters an observer callback.
        /// </summary>
        /// <param name="observer">Observer to unregister.</param>
        void Detach(EventHandler observer);
    }
}
