namespace FreelyProgrammableControl.Logic
{
    internal class Outputs : Subject, IOutputs
    {
        #region  fields
        private readonly IOutputDevice[] devices;
        #endregion fields

        #region  properties
        public int Length => devices.Length;
        public IOutputDevice this[int index]
        {
            get => devices[index];
            set => devices[index] = value;
        }
        #endregion properties

        #region constructors
        public Outputs(int length)
        {
            devices = new IOutputDevice[Math.Max(length, 0)];
            for (int i = 0; i < devices.Length; i++)
            {
                devices[i] = new DefaultOutputDevice() { Label = $"Output {i,2}" };
            }
        }
        #endregion constructors

        #region  methods
        public void Reset()
        {
            for (int i = 0; i < devices.Length; i++)
            {
                devices[i].Value = false;
            }
            Notify();
        }
        public void SetValue(int position, bool value)
        {
            devices[position].Value = value;
            Notify();
        }
        public bool GetValue(int position)
        {
            return devices[position].Value;
        }
        #endregion methods
    }
}
