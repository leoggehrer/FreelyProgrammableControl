
using FreelyProgrammableControl.Logic.Extensions;

namespace FreelyProgrammableControl.Logic
{
    /// <summary>
    /// Represents a parsed line of instruction from a source input.
    /// </summary>
    public class ParsedLine
    {
        /// <summary>
        /// Gets the line number associated with the current instance.
        /// </summary>
        /// <value>
        /// An integer representing the line number.
        /// </value>
        public int LineNumber { get; }
        /// <summary>
        /// Gets or sets a value indicating whether the item is a comment.
        /// </summary>
        /// <value>
        /// <c>true</c> if the item is a comment; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property can only be set within the same assembly.
        /// </remarks>
        public bool IsComment { get; internal set; }
        /// <summary>
        /// Gets a value indicating whether there is an error.
        /// </summary>
        /// <value>
        /// <c>true</c> if there is an error; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property is set internally and cannot be modified from outside the class.
        /// </remarks>
        public bool HasError { get; internal set; }
        /// <summary>
        /// Gets or sets the error message associated with the property.
        /// </summary>
        /// <value>
        /// A string that contains the error message. This property can be null.
        /// </value>
        /// <remarks>
        /// This property is set internally and can be accessed only within the same assembly.
        /// </remarks>
        public string? ErrorMessage { get; internal set; }
        /// <summary>
        /// Gets the source of the data.
        /// </summary>
        /// <value>
        /// A string representing the source.
        /// </value>
        public string Source { get; }
        /// <summary>
        /// Gets or sets the instruction text.
        /// </summary>
        /// <value>
        /// A string that represents the instruction. The default value is an empty string.
        /// </value>
        internal string Instruction { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// A string representing the subject. The default value is an empty string.
        /// </value>
        internal string Subject { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the address value.
        /// </summary>
        /// <value>
        /// An integer representing the address. The default value is 0.
        /// </value>
        internal int Address { get; set; } = 0;
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// An integer representing the current value. The default is 0.
        /// </value>
        internal int Value { get; set; } = 0;
        /// <summary>
        /// Initializes a new instance of the <see cref="ParsedLine"/> class.
        /// </summary>
        /// <param name="lineNumber">The line number of the source code.</param>
        /// <param name="source">The source code line as a string.</param>
        /// <remarks>
        /// This constructor parses the provided source code line into an instruction
        /// and performs analysis on that instruction.
        /// </remarks>
        public ParsedLine(int lineNumber, string source)
        {
            LineNumber = lineNumber;
            Source = source;
            Instruction = ToInstruction(source);
            AnalyzeInstruction();
        }
        /// <summary>
        /// Converts a given instruction string into a standardized format.
        /// </summary>
        /// <param name="source">The source string to be converted, which may contain leading or trailing spaces and can have mixed case letters.</param>
        /// <returns>A standardized string representation of the instruction, with specific formatting applied to certain items.</returns>
        /// <remarks>
        /// The method processes the input string by removing extra spaces, converting it to uppercase,
        /// and splitting it into individual items. If the second item in the list starts with specific
        /// letters ('I', 'O', 'M', 'T', or 'C') and contains a digit, it is further processed to separate
        /// the letter and the number. All other items are added
        private static string ToInstruction(string source)
        {
            var result = new List<string>();
            var items = source.RemoveLeftAndRight(' ')
                               .ToUpper()
                               .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < items.Length; i++)
            {
                if (i == 1
                    && (items[i].StartsWith('I') || items[i].StartsWith('O') || items[i].StartsWith('M') || items[i].StartsWith('T') || items[i].StartsWith('C'))
                    && items[i].ContainsDigit())
                {
                    result.Add(items[i][..1]);
                    result.Add(items[i].ToInt().ToString());
                }
                else
                {
                    result.Add(items[i]);
                }
            }
            return string.Join(' ', result);
        }
        /// <summary>
        /// Analyzes the current instruction and updates the state of the object based on the instruction's content.
        /// </summary>
        /// <remarks>
        /// The method checks if the instruction is a comment (starts with '#'). If it is not a comment, it parses the instruction
        /// to determine the operation to be performed, which can include various commands such as GET, GETNOT, DUP, MOV,
        /// SET, INC, DEC, CMP, GT, and LE. The method updates the properties <see cref="Instruction"/>, <see cref="Subject"/>,
        /// <see cref="Address"/>, and <see cref="Value"/> based on the parsed instruction. It also handles errors and sets
        /// <see cref
        private void AnalyzeInstruction()
        {
            IsComment = Instruction.StartsWith('#');
            if (IsComment == false)
            {
                var items = Instruction.Split(' ');

                try
                {
                    // e.g.: GET 1 => stack.push(true)
                    // e.g.: GET 0 => stack.push(false)
                    if (items.Length == 2 && (items[0] == "G" || items[0] == "GET"))
                    {
                        Instruction = "GET";
                        Subject = "C_OPD";  // C_OPD = Constant Operand
                        Address = 0;
                        Value = items[1] == "1" ? 1 : 0;
                    }
                    // e.g.: GET I 10 => stack.push(inputs.GetValue(10))
                    // e.g.: GET O 10 => stack.push(outputs.GetValue(10))
                    // e.g.: GET M 10 => stack.push(memory.GetValue(10))
                    // e.g.: GET T 10 => stack.push(timers.GetValue(10))
                    else if (items.Length == 3 && (items[0] == "G" || items[0] == "GET")
                             && (items[1] == "I" || items[1] == "O" || items[1] == "M" || items[1] == "T"))
                    {
                        Instruction = "GET";
                        Subject = items[1];  // I || O || M || T
                        Address = Convert.ToInt32(items[2]);
                        Value = 0;
                    }
                    // e.g.: GETNOT I 10 => stack.push(!inputs.GetValue(10))
                    // e.g.: GETNOT O 10 => stack.push(!outputs.GetValue(10))
                    // e.g.: GETNOT M 10 => stack.push(!memory.GetValue(10))
                    // e.g.: GETNOT T 10 => stack.push(!timers.GetValue(10))
                    else if (items.Length == 3 && (items[0] == "GN" || items[0] == "GETNOT")
                             && (items[1] == "I" || items[1] == "O" || items[1] == "M" || items[1] == "T"))
                    {
                        Instruction = "GETNOT";
                        Subject = items[1];  // I || O || M || T
                        Address = Convert.ToInt32(items[2]);
                        Value = 0;
                    }
                    // e.g.: DUP => stack.push(stack.top())
                    else if (items.Length == 1 && (items[0] == "D" || items[0] == "DUP"))
                    {
                        Instruction = "DUP";
                        Subject = "OP";  // OP = Operator
                        Address = 0;
                        Value = 2;
                    }
                    // e.g.: DUP 3 => stack.push(stack.top())
                    //                stack.push(stack.top()) 
                    else if (items.Length == 2 && (items[0] == "D" || items[0] == "DUP"))
                    {
                        Instruction = "DUP";
                        Subject = "OP";  // OP = Operator
                        Address = 0;
                        Value = Convert.ToInt32(items[1]);
                    }
                    // e.g.: NOT => stack.push(!stack.pop())
                    else if (items.Length == 1 && (items[0] == "N" || items[0] == "NOT"))
                    {
                        Instruction = "NOT";
                        Subject = "OP";  // OP = Operator
                        Address = 0;
                        Value = 0;
                    }
                    // e.g.: AND => stack.push(stack.pop() && stack.pop())
                    else if (items.Length == 1 && (items[0] == "A" || items[0] == "AND"))
                    {
                        Instruction = "AND";
                        Subject = "OP";  // OP = Operator
                        Address = 0;
                        Value = 0;
                    }
                    // e.g.: OR => stack.push(stack.pop() || stack.pop())
                    else if (items.Length == 1 && (items[0] == "O" || items[0] == "OR"))
                    {
                        Instruction = "OR";
                        Subject = "OP";  // OP = Operator
                        Address = 0;
                        Value = 0;
                    }
                    // e.g.: XOR => stack.push(stack.pop() ^ stack.pop())
                    else if (items.Length == 1 && (items[0] == "X" || items[0] == "XOR"))
                    {
                        Instruction = "XOR";
                        Subject = "OP";  // OP = Operator
                        Address = 0;
                        Value = 0;
                    }
                    // e.g.: MOV O 10 => outputs.SetValue(10, stack.pop())
                    // e.g.: MOV M 10 => memory.SetValue(10, stack.pop())
                    else if (items.Length == 3 && (items[0] == "M" || items[0] == "MOV")
                             && (items[1] == "O" || items[1] == "M"))
                    {
                        Instruction = "MOV";
                        Subject = items[1];  // O || M
                        Address = Convert.ToInt32(items[2]);
                        Value = 0;
                    }
                    // e.g.: CMOV O 10 = stack.pop() == true => outputs.SetValue(10, stack.pop())
                    // e.g.: CMOV M 10 = stack.pop() == true => memory.SetValue(10, stack.pop())
                    else if (items.Length == 4 && (items[0] == "CM" || items[0] == "CMOV")
                             && (items[1] == "O" || items[1] == "M"))
                    {
                        Instruction = "CMOV";
                        Subject = items[1];  // O || M
                        Address = Convert.ToInt32(items[2]);
                        Value = items[3] == "1" ? 1 : 0;
                    }
                    // e.g.: SET T 10 1500 => timers.SetValue(10, 1500)
                    // e.g.: SET C 10 100 => counters.SetValue(10, 100)
                    else if (items.Length == 4 && (items[0] == "S" || items[0] == "SET")
                             && (items[1] == "T" || items[1] == "C"))
                    {
                        Instruction = "SET";
                        Subject = items[1];
                        Address = Convert.ToInt32(items[2]);
                        Value = Convert.ToInt32(items[3]);
                    }
                    // e.g.: CSET T 10 1500 = stack.pop() == true => timers.SetValue(10, 1500)
                    // e.g.: CSET C 10 100 = stack.pop() == true => counters.SetValue(10, 100)
                    else if (items.Length == 4 && (items[0] == "CS" || items[0] == "CSET")
                             && (items[1] == "T" || items[1] == "C"))
                    {
                        Instruction = "CSET";
                        Subject = items[1];
                        Address = Convert.ToInt32(items[2]);
                        Value = Convert.ToInt32(items[3]);
                    }
                    // e.g.: INC C 10 1 => if (statck.pop() == true) => counters.SetValue(10, counters.GetValue(10) + 1)
                    else if (items.Length == 3 && (items[0] == "I" || items[0] == "INC")
                             && (items[1] == "C"))
                    {
                        Instruction = "INC";
                        Subject = items[1];
                        Address = Convert.ToInt32(items[2]);
                        Value = 1;
                    }
                    // e.g.: DEC C 10 0 => if (statck.pop() == false) => counters.SetValue(10, counters.GetValue(10) - 1)
                    else if (items.Length == 4 && (items[0] == "D" || items[0] == "DEC")
                             && (items[1] == "C"))
                    {
                        Instruction = "DEC";
                        Subject = items[1];
                        Address = Convert.ToInt32(items[2]);
                        Value = items[1] == "1" ? 1 : 0;
                    }
                    // e.g.: CMP C 10 17 => stack.pop(counters.GetValue(10) == 17)
                    else if (items.Length == 4 && (items[0] == "C" || items[0] == "CMP")
                             && (items[1] == "C"))
                    {
                        Instruction = "CMP";
                        Subject = items[1];
                        Address = Convert.ToInt32(items[2]);
                        Value = Convert.ToInt32(items[3]);
                    }
                    // e.g.: GT C 10 17 => stack.pop(counters.GetValue(10) > 17)
                    else if (items.Length == 4 && (items[0] == "GT")
                             && (items[1] == "C"))
                    {
                        Instruction = "GT";
                        Subject = items[1];
                        Address = Convert.ToInt32(items[2]);
                        Value = Convert.ToInt32(items[3]);
                    }
                    // e.g.: LE C 10 17 => stack.pop(counters.GetValue(10) < 17)
                    else if (items.Length == 4 && (items[0] == "LE")
                             && (items[1] == "C"))
                    {
                        Instruction = "LE";
                        Subject = items[1];
                        Address = Convert.ToInt32(items[2]);
                        Value = Convert.ToInt32(items[3]);
                    }
                    else
                    {
                        HasError = true;
                        ErrorMessage = "Unknown instruction";
                    }
                }
                catch (Exception ex)
                {
                    HasError = true;
                    ErrorMessage = ex.Message;
                }
            }
        }
    }
}
