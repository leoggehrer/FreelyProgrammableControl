namespace FreelyProgrammableControl.Logic
{
    internal class Inputs : Subject
    {
        #region  fields
        private readonly IInputDevice[] devices;
        #endregion fields

        #region  properties
        public int Length => devices.Length;
        public IInputDevice this[int index]
        {
            get => devices[index];
            set => devices[index] = value;
        }
        #endregion properties

        #region constructors
        public Inputs(int length)
        {
            devices = new IInputDevice[Math.Max(length, 0)];
            for (int i = 0; i < devices.Length; i++)
            {
                devices[i] = new Switch() { Label = $"Switch {i, 2}" };
                devices[i].Attach((sender, e) => Notify());
            }
        }
        #endregion constructors

        #region  methods
        public bool GetValue(int position)
        {
            return devices[position].Value;
        }
        #endregion methods
    }
}
