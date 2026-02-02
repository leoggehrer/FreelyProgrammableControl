using System.ComponentModel;
using FreelyProgrammableControl.Logic.Execution;
using FreelyProgrammableControl.McpTool.Models;
using ModelContextProtocol.Server;

namespace FreelyProgrammableControl.McpTool.Tools
{
    /// <summary>
    /// Tool for parsing FPC (Freely Programmable Control) programs.
    /// Allows AI agents to validate programs and receive error feedback.
    /// </summary>
    [McpServerToolType]
    public static partial class ProgramParserTool
    {
        /// <summary>
        /// Parses a FPC program and validates its syntax.
        /// Returns detailed information about parse errors if any occur.
        /// </summary>
        /// <param name="programCode">The FPC program code to parse, with lines separated by newlines.</param>
        /// <returns>Parse result containing validation status and error details.</returns>
        [McpServerTool(Name = "parse_fpc_program")]
        [Description("Parses a FPC program and validates its syntax. Returns detailed error information if parsing fails.")]
        public static ParseResult ParseProgram(            
            [Description("The FPC program code to parse, with lines separated by newlines.")]string programCode)
        {
            try
            {
                var lines = programCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var parsedLines = ExecutionUnit.Parse(lines);

                var errors = parsedLines
                    .Where(pl => pl.HasError)
                    .Select(pl => new ParseError
                    {
                        LineNumber = pl.LineNumber,
                        SourceCode = pl.Source,
                        ErrorMessage = pl.ErrorMessage ?? "Unknown error"
                    })
                    .ToList();

                if (errors.Count > 0)
                {
                    return new ParseResult
                    {
                        IsValid = false,
                        ErrorCount = errors.Count,
                        Errors = errors,
                        Message = $"Program has {errors.Count} parse error(s)."
                    };
                }

                return new ParseResult
                {
                    IsValid = true,
                    ErrorCount = 0,
                    Errors = [],
                    TotalLines = parsedLines.Length,
                    NonCommentLines = parsedLines.Count(pl => !pl.IsComment),
                    Message = $"Program parsed successfully! Total lines: {parsedLines.Length}, Instructions: {parsedLines.Count(pl => !pl.IsComment)}"
                };
            }
            catch (Exception ex)
            {
                return new ParseResult
                {
                    IsValid = false,
                    ErrorCount = 1,
                    Errors = new List<ParseError>
                    {
                        new ParseError
                        {
                            LineNumber = -1,
                            ErrorMessage = $"Parsing failed: {ex.Message}"
                        }
                    },
                    Message = $"Parsing failed with exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Parses a FPC program and loads it into an ExecutionUnit for execution testing.
        /// </summary>
        /// <param name="programCode">The FPC program code to parse and load.</param>
        /// <param name="inputCount">Number of inputs available (default: 64).</param>
        /// <param name="outputCount">Number of outputs available (default: 64).</param>
        /// <returns>Validation result with execution unit status.</returns>
        [McpServerTool(Name = "validate_fpc_execution")]
        [Description("Parses a FPC program and loads it into an ExecutionUnit. Validates that the program can be executed properly.")]
        public static ExecutionValidationResult ValidateProgramExecution(
            [Description("The FPC program code to validate.")]
            string programCode,
            [Description("Number of input devices (default: 64).")]
            int inputCount = 64,
            [Description("Number of output devices (default: 64).")]
            int outputCount = 64)
        {
            try
            {
                var executionUnit = new ExecutionUnit(inputCount, outputCount);
                var lines = programCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                
                executionUnit.LoadSource(lines);

                var result = new ExecutionValidationResult
                {
                    IsValid = !executionUnit.HasParseError,
                    InputCount = inputCount,
                    OutputCount = outputCount,
                    TotalSourceLines = executionUnit.Source.Length,
                    MemorySize = executionUnit.MemoryLength,
                    TimerCount = executionUnit.TimerLength
                };

                if (executionUnit.HasParseError)
                {
                    result.ParseErrorMessage = executionUnit.ParseErrorMessage ?? "Unknown parse error";
                    result.Message = $"Execution validation failed: {result.ParseErrorMessage}";
                }
                else
                {
                    result.Message = "Program is ready for execution!";
                    result.CanStart = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ExecutionValidationResult
                {
                    IsValid = false,
                    Message = $"Validation failed: {ex.Message}",
                    ParseErrorMessage = ex.Message
                };
            }
        }
    }
 }
