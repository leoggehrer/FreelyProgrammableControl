namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents a collection of output devices and provides methods to manage their states.
    /// </summary>
    /// <remarks>
    /// This class inherits from <see cref="Subject"/> and implements the <see cref="IOutputs"/> interface.
    /// It provides indexed access to a collection of output devices and allows for resetting and setting values of those devices.
    /// </remarks>
    internal class Outputs : Subject, IOutputs
    {
        #region  fields
        private readonly IOutputDevice[] devices;
        #endregion fields

        #region  properties
        /// <summary>
        /// Gets the number of devices in the collection.
        /// </summary>
        /// <value>The length of the devices array.</value>
        public int Length => devices.Length;
        public IOutputDevice this[int index]
        {
            get => devices[index];
            set => devices[index] = value;
        }
        #endregion properties

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Outputs"/> class with a specified number of output devices.
        /// </summary>
        /// <param name="length">The number of output devices to create. If the value is less than zero, zero devices will be created.</param>
        /// <remarks>
        /// Each output device is initialized with a default label in the format "Output X", where X is the index of the device.
        /// An event handler is attached to each device to notify when an event occurs.
        /// </remarks>
        public Outputs(int length)
        {
            devices = new IOutputDevice[Math.Max(length, 0)];
            for (int i = 0; i < devices.Length; i++)
            {
                devices[i] = new DefaultOutputDevice() { Label = $"Output {i,2}" };
                devices[i].Attach((sender, e) => Notify());
            }
        }
        #endregion constructors

        #region  methods
        /// <summary>
        /// Resets the state of all devices by setting their values to false.
        /// </summary>
        /// <remarks>
        /// This method iterates through the array of devices and sets the Value property of each device to false.
        /// After resetting the values, it calls the <see cref="NotifyAsync"/> method to notify any listeners of the change.
        /// </remarks>
        public void Reset()
        {
            for (int i = 0; i < devices.Length; i++)
            {
                devices[i].Value = false;
            }
            NotifyAsync();
        }
        /// <summary>
        /// Sets the value of the device at the specified position.
        /// </summary>
        /// <param name="position">The zero-based index of the device in the devices collection.</param>
        /// <param name="value">The boolean value to set for the device.</param>
        /// <remarks>
        /// If the new value is different from the current value, the method will trigger an asynchronous notification.
        /// </remarks>
        public void SetValue(int position, bool value)
        {
            var save = devices[position].Value;

            devices[position].Value = value;
            if (save != value)
                NotifyAsync();
        }
        /// <summary>
        /// Retrieves the value of the device at the specified position.
        /// </summary>
        /// <param name="position">The zero-based index of the device in the list.</param>
        /// <returns>
        /// <c>true</c> if the device at the specified position has a value of true; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the position is outside the bounds of the devices collection.</exception>
        public bool GetValue(int position)
        {
            return devices[position].Value;
        }
        /// <summary>
        /// Calculates the hash code for the current instance based on the values of the devices array.
        /// </summary>
        /// <returns>
        /// An integer hash code that represents the current instance.
        /// </returns>
        /// <remarks>
        /// This method iterates through the <c>devices</c> array, multiplying the current result by 2 and adding
        /// 1 for each device that has a <c>Value</c> of <c>true</c>. The resulting integer is a simple hash code
        /// representation of the device states.
        /// </remarks>
        public override int GetHashCode()
        {
            int result = 0;

            for (int i = 0; i < devices.Length; i++)
            {
                result *= 2;
                result += devices[i].Value ? 1 : 0;
            }
            return result;
        }
        #endregion methods
    }
}
