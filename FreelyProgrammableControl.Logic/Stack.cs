namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents a generic stack data structure that allows
    /// adding and removing elements in a last-in-first-out (LIFO) manner.
    /// </summary>
    /// <typeparam name="T">The type of elements in the stack.</typeparam>
    internal class Stack<T>
    {
        #region nested class
        private class Element(T data, Element? next)
        {
            /// <summary>
            /// Gets or sets the data associated with this property.
            /// </summary>
            /// <typeparam name="T">The type of the data.</typeparam>
            public T Data { get; set; } = data;
            /// <summary>
            /// Gets or sets the next <see cref="Element"/> in the sequence.
            /// </summary>
            /// <value>
            /// An instance of <see cref="Element"/> that represents the next element, or <c>null</c> if there is no next element.
            /// </value>
            public Element? Next { get; set; } = next;
        }
        #endregion  nested class

        #region  fields
        private Element? head = null;
        #endregion fields

        #region  properties
        /// <summary>
        /// Gets a value indicating whether the collection is empty.
        /// </summary>
        /// <value>
        /// <c>true</c> if the collection has no elements; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => head == null;
        #endregion properties

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Stack"/> class.
        /// </summary>
        public Stack()
        {
        }
        #endregion constructors

        #region  methods
        /// <summary>
        /// Clears the contents of the data structure by setting the head to null.
        /// </summary>
        /// <remarks>
        /// This method effectively removes all elements from the data structure,
        /// making it empty. After calling this method, the data structure will
        /// no longer contain any references to the previous elements.
        /// </remarks>
        public void Clear()
        {
            head = null;
        }
        /// <summary>
        /// Retrieves the top element of the stack without removing it.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the stack.</typeparam>
        /// <returns>The top element of the stack.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the stack is empty.</exception>
        public T Top()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Stack is empty!");
            }

            return head!.Data;
        }
        /// <summary>
        /// Removes and returns the top element of the stack.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the stack.</typeparam>
        /// <returns>The data of the top element of the stack.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the stack is empty.</exception>
        public T Pop()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Stack is empty!");
            }

            Element tmp = head!;

            head = tmp.Next;
            return tmp.Data;
        }
        /// <summary>
        /// Adds a new element with the specified data to the top of the stack.
        /// </summary>
        /// <param name="data">The data to be added to the stack.</param>
        /// <remarks>
        /// This method creates a new element and sets it as the new head of the stack,
        /// effectively pushing the new data onto the stack.
        /// </remarks>
        public void Push(T data)
        {
            head = new Element(data, head);
        }
        #endregion methods
    }
}
