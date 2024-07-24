namespace FreelyProgrammableControl.Logic
{
    public class Switch : IInputDevice
    {
        #region fields
        private bool _value = false;
        #endregion fields

        #region  properties
        public bool Value 
        {
            get => _value;
        }
        #endregion  properties

        #region methods
        public void Toggle()
        {
            _value = !_value;
        }
        #endregion methods
    }
}
