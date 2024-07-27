
using FreelyProgrammableControl.Logic.Extensions;

namespace FreelyProgrammableControl.Logic
{
    public class ParsedLine
    {
        public int LineNumber { get; }
        public bool IsComment { get; internal set; }
        public bool HasError { get; internal set; }
        public string Source { get; }
        internal string Instruction { get; set; } = string.Empty;
        internal string Subject { get; set; } = string.Empty;
        internal int Address { get; set; } = 0;
        internal int Value { get; set; } = 0;
        public ParsedLine(int lineNumber, string source)
        {
            LineNumber = lineNumber;
            Source = source;
            Instruction = source.Trim()
                                .RemoveLeftAndRight(' ')
                                .ToUpper();
            AnalyzeInstruction();
        }

        private void AnalyzeInstruction()
        {
            IsComment = Instruction.StartsWith('#');
            if (IsComment)
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
                        Subject = "V_OPD";  // V_OPD = Variable Operand
                        Address = Convert.ToInt32(items[2]);
                        Value = 0;
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






                    // e.g.: SET T 10 1500 => timers.SetValue(10, 1500)
                    // e.g.: SET C 10 100 => counters.SetValue(10, 100)
                    else if (items.Length == 4 && (items[0] == "C" || items[0] == "CMP")
                             && (items[1] == "I" || items[1] == "O" || items[1] == "M" || items[1] == "T"))
                    {
                        Instruction = "SET";
                        Subject = items[1];
                        Address = Convert.ToInt32(items[2]);
                        Value = Convert.ToInt32(items[3]);
                    }

                    else if (items.Length == 3 && items[0] == "G" 
                             && (items[1] == "I" || items[1] == "O" || items[1] == "M" || items[1] == "T" || items[1] == "C"))
                    {
                        Instruction = "GET";
                        Subject = items[1];
                        Address = Convert.ToInt32(items[2]);
                        Value = 0;
                    }
                    else if (items.Length == 4 && items[0] == "S"
                             && (items[1] == "O" || items[1] == "M" || items[1] == "T" || items[1] == "C"))
                    {
                        Instruction = "SET";
                        Subject = items[1];
                        Address = Convert.ToInt32(items[2]);
                        Value = items[3] == "T" ? 1 : 0;
                    }
                    else if (items.Length == 4 && items[0] == "S"
                             && (items[1] == "O" || items[1] == "M" || items[1] == "T" || items[1] == "C"))
                    {
                        Instruction = "SET";
                        Subject = items[1];
                        Address = Convert.ToInt32(items[2]);
                        Value = items[3] == "T" ? 1 : 0;
                    }

                }
                catch (Exception ex)
                {

                    throw;
                }
            }
        }
    }
}
