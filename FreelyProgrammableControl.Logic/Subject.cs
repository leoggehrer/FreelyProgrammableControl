namespace FreelyProgrammableControl.Logic
{
    public abstract class Subject : ISubject
    {
        #region fields
        private readonly List<EventHandler> observers = [];
        #endregion fields

        #region  properties
        public int Count => observers.Count;
        #endregion properties

        #region constructors
        #endregion constructors

        #region methods
        public void Attach(EventHandler observer)
        {
            observers.Add(observer);
        }

        public void Detach(EventHandler observer)
        {
            observers.Remove(observer);
        }

        protected void Notify()
        {
            foreach (EventHandler observer in observers)
            {
                observer.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion methods
    }
}
