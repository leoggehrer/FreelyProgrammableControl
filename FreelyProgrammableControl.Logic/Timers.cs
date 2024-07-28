﻿namespace FreelyProgrammableControl.Logic
{
    internal class Timers(int length)
    {
        #region nested class
        private class Timer(DateTime dateTime, int durationInMs)
        {
            public DateTime DateTime { get; set; } = dateTime;
            public int DurationInMs { get; set; } = durationInMs;
            public bool Value
            {
                get
                {
                    var totalInMs = (DateTime.Now - DateTime).TotalMilliseconds;
                    
                    return totalInMs <= DurationInMs;
                }
            }
        }
        #endregion  nested class

        #region fields
        private readonly Timer?[] timers = new Timer[Math.Max(length, 0)];
        #endregion fields

        #region  properties
        public int Length => timers.Length;
        #endregion properties

        #region constructors
        #endregion constructors

        #region methods
        public void Reset()
        {
            for (int i = 0; i < timers.Length; i++)
            {
                timers[i] = null;
            }
        }

        public void SetTimer(int position, int durationInMs)
        {
            timers[position] = new Timer(DateTime.Now, durationInMs);
        }

        public bool GetValue(int position)
        {
            return timers[position] == default ? false : timers[position]!.Value;
        }
        #endregion methods
    }
}
