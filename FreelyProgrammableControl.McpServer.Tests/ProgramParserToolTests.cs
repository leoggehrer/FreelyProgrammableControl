using FreelyProgrammableControl.McpServer.Tools;

namespace FreelyProgrammableControl.McpServer.Tests;

public class ProgramParserToolTests
{
    [Fact]
    public void ParseProgram_ValidSimpleProgram_ReturnsIsValidTrue()
    {
        var code = "GET I 0\nMOV O 0";

        var result = ProgramParserTool.ParseProgram(code);

        Assert.True(result.IsValid);
        Assert.Equal(0, result.ErrorCount);
        Assert.Empty(result.Errors);
        Assert.Equal(2, result.NonCommentLines);
    }

    [Fact]
    public void ParseProgram_CountsCommentsAndBlanksCorrectly()
    {
        var code = "# comment\nGET I 0\n\nMOV O 0";

        var result = ProgramParserTool.ParseProgram(code);

        Assert.True(result.IsValid);
        Assert.Equal(4, result.TotalLines);
        Assert.True(result.NonCommentLines < result.TotalLines);
    }

    [Fact]
    public void ParseProgram_UnknownInstruction_ReturnsError()
    {
        var code = "GET I 0\nFOO BAR\nAND";

        var result = ProgramParserTool.ParseProgram(code);

        Assert.False(result.IsValid);
        Assert.Equal(1, result.ErrorCount);
        Assert.Contains(result.Errors, e => e.SourceCode == "FOO BAR");
        Assert.Contains("parse error", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseProgram_MultipleErrors_ReportsAll()
    {
        var code = "FOO\nBAR\nBAZ";

        var result = ProgramParserTool.ParseProgram(code);

        Assert.False(result.IsValid);
        Assert.Equal(3, result.ErrorCount);
    }

    [Fact]
    public void ParseProgram_EmptyProgram_IsValid()
    {
        var result = ProgramParserTool.ParseProgram(string.Empty);

        Assert.True(result.IsValid);
        Assert.Equal(0, result.NonCommentLines);
    }

    [Fact]
    public void ParseProgram_HandlesCrlfAndCrLineEndings()
    {
        var lfResult = ProgramParserTool.ParseProgram("GET I 0\nMOV O 0");
        var crlfResult = ProgramParserTool.ParseProgram("GET I 0\r\nMOV O 0");
        var crResult = ProgramParserTool.ParseProgram("GET I 0\rMOV O 0");

        Assert.True(lfResult.IsValid);
        Assert.True(crlfResult.IsValid);
        Assert.True(crResult.IsValid);
        Assert.Equal(lfResult.NonCommentLines, crlfResult.NonCommentLines);
        Assert.Equal(lfResult.NonCommentLines, crResult.NonCommentLines);
    }

    [Fact]
    public void ValidateProgramExecution_ValidProgram_CanStart()
    {
        var code = "GET I 0\nMOV O 0";

        var result = ProgramParserTool.ValidateProgramExecution(code);

        Assert.True(result.IsValid);
        Assert.True(result.CanStart);
        Assert.Null(result.ParseErrorMessage);
        Assert.Equal(128, result.InputCount);
        Assert.Equal(128, result.OutputCount);
    }

    [Fact]
    public void ValidateProgramExecution_InvalidProgram_FailsValidation()
    {
        var code = "GET I 0\nFOO BAR";

        var result = ProgramParserTool.ValidateProgramExecution(code);

        Assert.False(result.IsValid);
        Assert.False(result.CanStart);
        Assert.NotNull(result.ParseErrorMessage);
    }

    [Fact]
    public void ValidateProgramExecution_CustomInputOutputCounts_ReflectedInResult()
    {
        var code = "GET I 0";

        var result = ProgramParserTool.ValidateProgramExecution(code, inputCount: 16, outputCount: 8);

        Assert.Equal(16, result.InputCount);
        Assert.Equal(8, result.OutputCount);
    }

    [Fact]
    public void ParseProgram_MotorSelbsthaltung_Valid()
    {
        var code = string.Join('\n',
            "GET I 2",
            "CMOV M 0 0",
            "GET I 1",
            "CMOV M 0 0",
            "GET I 0",
            "GETNOT I 1",
            "AND",
            "GETNOT I 2",
            "AND",
            "CMOV M 0 1",
            "GET M 0",
            "MOV O 0");

        var result = ProgramParserTool.ParseProgram(code);

        Assert.True(result.IsValid);
        Assert.Equal(12, result.NonCommentLines);
    }
}
