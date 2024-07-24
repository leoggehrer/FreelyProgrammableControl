namespace FreelyProgrammableControl.Logic
{
    internal class Stack<T>
    {
        #region nested class
        private class Element(T data, Element? next)
        {
            public T Data { get; set; } = data;
            public Element? Next { get; set; } = next;
        }
        #endregion  nested class

        #region  fields
        private Element? head = null;
        #endregion fields

        #region  properties
        public bool IsEmpty => head == null;
        #endregion properties

        #region constructors
        public Stack()
        {
        }
        #endregion constructors

        #region  methods
        public void Clear()
        {
            head = null;
        }
        public T Top()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Stack is empty!");
            }

            return head!.Data;
        }
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
        public void Push(T data)
        {
            head = new Element(data, head);
        }
        #endregion methods
    }
}
