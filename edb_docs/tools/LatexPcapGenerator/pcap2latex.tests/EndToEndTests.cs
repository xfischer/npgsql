using Spectre.Console;
using Xunit.Abstractions;

namespace pcap2latex.tests;

// TODO: fix console pollution with previous tests, causes failures
//
[CollectionDefinition(nameof(CommandLine_EndToEndTests), DisableParallelization = true)]
public class CommandLine_EndToEndTests(ITestOutputHelper output)
{

    [Fact]
    public void CommandLine_EmptyArgs()
    {
        // arrange
        string[] args = [];
        AnsiConsole.Console = new Spectre.Console.Testing.TestConsole();
        AnsiConsole.Record();

        // act
        var result = pgcap2latex.Program.Main(args);

        // assert
        Assert.Equal(1, result);
        var text = AnsiConsole.ExportText();
        output.WriteLine(text);
        Assert.StartsWith("USAGE:", text);
    }

    [Fact]
    public void CommandLine_ValidFile()
    {
        // arrange
        string[] args = ["TestData/extendedQuery.pcapng", "5432"];
        AnsiConsole.Console = new Spectre.Console.Testing.TestConsole();
        AnsiConsole.Record();

        // act
        var result = pgcap2latex.Program.Main(args);

        // assert
        Assert.Equal(0, result);
        var text = AnsiConsole.ExportText();
        output.WriteLine(text);
        Assert.EndsWith("3 packet(s) processed. 21 messages written.", text);
    }


    [Fact]
    public void CommandLine_ValidFile_InvalidPort()
    {
        // arrange
        string[] args = ["TestData/extendedQuery.pcapng", "1000"];
        AnsiConsole.Console = new Spectre.Console.Testing.TestConsole();
        AnsiConsole.Record();

        // act
        var result = pgcap2latex.Program.Main(args);

        // assert
        Assert.Equal(0, result);
        var text = AnsiConsole.ExportText();
        output.WriteLine(text);
        Assert.Contains("0 packet(s) processed. 0 messages written.", text);
    }

    [Fact]
    public void CommandLine_InvalidFile()
    {
        // arrange
        string[] args = ["TestData/DOES_NOT_EXISTS.pcapng", "5432"];
        AnsiConsole.Console = new Spectre.Console.Testing.TestConsole();
        AnsiConsole.Record();

        // act
        var result = pgcap2latex.Program.Main(args);

        // assert
        Assert.Equal(-1, result);
        var text = AnsiConsole.ExportText();
        output.WriteLine(text);
        Assert.Contains("CommandRuntimeException: Input file ", text);
    }
}
