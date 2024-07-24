namespace FreelyProgrammableControl.Logic
{
    internal class Outputs(int length) : Subject
    {
        #region  fields
        private readonly bool[] values = new bool[Math.Max(length, 0)];
        #endregion fields

        #region  properties
        public int Length => values.Length;
        #endregion properties

        #region constructors
        #endregion constructors

        #region  methods
        public void Reset()
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = false;
            }
            Notify();
        }
        public void SetValue(int position, bool value)
        {
            values[position] = value;
            Notify();
        }
        public bool GetValue(int position)
        {
            return values[position];
        }
        #endregion methods
    }
}
