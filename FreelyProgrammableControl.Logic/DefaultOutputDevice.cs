namespace FreelyProgrammableControl.Logic
{
    public class DefaultOutputDevice : IOutputDevice
    {
        private bool _value;

        public required string Label { get; set; }
        public bool Value 
        { 
            get => _value; 
            set => _value = value; 
        }
    }
}
