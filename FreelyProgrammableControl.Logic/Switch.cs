namespace FreelyProgrammableControl.Logic
{
    public class Switch : Subject, IInputDevice
    {
        #region fields
        private bool _value = false;
        #endregion fields

        #region  properties
        public required string Label { get; set; }
        public bool Value 
        {
            get => _value;
        }
        #endregion  properties

        #region methods
        public void Toggle()
        {
            _value = !_value;
            Notify();
        }
        #endregion methods
    }
}
