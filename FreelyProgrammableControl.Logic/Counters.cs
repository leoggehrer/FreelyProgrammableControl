﻿namespace FreelyProgrammableControl.Logic
{
    internal class Counters(int length) : Subject
    {
        #region  fields
        private readonly int[] values = new int[Math.Max(length, 0)];
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
        public void SetValue(int position, int value)
        {
            values[position] = value;
            Notify();
        }
        public int GetValue(int position)
        {
            return values[position];
        }
        #endregion methods
    }
}
