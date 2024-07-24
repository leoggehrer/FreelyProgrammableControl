namespace FreelyProgrammableControl.Logic
{
    internal class Memory<T>(int length) : Subject
    {
        #region  fields
        private readonly T[] values = new T[Math.Max(length, 0)];
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
                values[i] = default!;
            }
            Notify();
        }
        public void SetValue(int position, T value)
        {
            values[position] = value;
            Notify();
        }
        public T GetValue(int position)
        {
            return values[position];
        }
        #endregion methods
    }
}
