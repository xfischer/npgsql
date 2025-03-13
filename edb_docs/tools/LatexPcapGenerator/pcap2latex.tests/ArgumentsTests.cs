using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using pgcap2latex;
using Spectre.Console;
using Spectre.Console.Cli;

namespace pcap2latex.tests;

public class ArgumentsTests
{
    [Fact]
    public void Execute_WithValidFile()
    {
        // arrange
        var settings = new ConvertCommand.Settings() { 
            InputFile = "TestData/extendedQuery.pcapng", 
            Multiple = false, 
            Port = 5446 };
        
        // act
        var result = settings.Validate();

        // assert
        Assert.True(result.Successful);
    }

    //[Fact]
    //public void Execute_WithEmptyList()
    //{
    //    // arrange
    //    var pcapService = Substitute.For<IPcapService>();
    //    var latexService = Substitute.For<IPcapToLatexService>();
    //    var app = Substitute.For<ConvertApp>(pcapService, latexService, NullLogger<ConvertApp>.Instance);
    //    var settings = new ConvertCommand.Settings() { InputFile = "TestData/extendedQuery.pcapng", Multiple = false, Port = 5446 };
    //    var command = new ConvertCommand(app);
    //    var context = new CommandContext(Enumerable.Empty<string>(), _remainingArgs, "convert", null);

    //    // act
    //    AnsiConsole.Record();
    //    command.Execute(context, settings);

    //    // assert
    //    var text = AnsiConsole.ExportText();
    //    Assert.Contains("# Students: 0", text);
    //}
}

