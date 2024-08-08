namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents a collection of input devices that can be monitored and interacted with.
    /// </summary>
    /// <remarks>
    /// The <see cref="Inputs"/> class inherits from <see cref="Subject"/> and implements <see cref="IInputs"/>.
    /// It provides access to a specified number of input devices and allows for retrieving their values.
    /// </remarks>
    internal class Inputs : Subject, IInputs
    {
        #region  fields
        private readonly IInputDevice[] devices;
        #endregion fields

        #region  properties
        /// <summary>
        /// Gets the number of devices in the collection.
        /// </summary>
        /// <value>
        /// The length of the devices array.
        /// </value>
        public int Length => devices.Length;
        public IInputDevice this[int index]
        {
            get => devices[index];
            set => devices[index] = value;
        }
        #endregion properties

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Inputs"/> class with the specified number of input devices.
        /// </summary>
        /// <param name="length">The number of input devices to create. If the specified length is less than zero, zero devices will be created.</param>
        /// <remarks>
        /// Each device is initialized as a <see cref="Switch"/> with a label formatted as "Switch {index}" where index is the device's position.
        /// An event handler is attached to each device that invokes the <see cref="Notify"/> method when the device is triggered.
        /// </remarks>
        public Inputs(int length)
        {
            devices = new IInputDevice[Math.Max(length, 0)];
            for (int i = 0; i < devices.Length; i++)
            {
                devices[i] = new Switch() { Label = $"Switch {i,2}" };
                devices[i].Attach((sender, e) => Notify());
            }
        }
        #endregion constructors

        #region  methods
        /// <summary>
        /// Retrieves the value of the device at the specified position.
        /// </summary>
        /// <param name="position">The zero-based index of the device in the collection.</param>
        /// <returns>
        /// <c>true</c> if the device at the specified position has a value of true; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when the specified position is outside the bounds of the devices collection.
        /// </exception>
        public bool GetValue(int position)
        {
            return devices[position].Value;
        }
        #endregion methods
    }
}
