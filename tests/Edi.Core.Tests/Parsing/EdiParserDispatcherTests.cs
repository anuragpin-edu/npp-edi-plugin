using System.Collections.Generic;
using Edi.Core.Dictionary;
using Edi.Core.Model;
using Edi.Core.Parsing;
using Xunit;

namespace Edi.Core.Tests.Parsing;

public class EdiParserDispatcherTests
{
    private class MockParser : IEdiParser
    {
        private readonly bool _canParse;
        private readonly EdiStandard _standard;

        public MockParser(bool canParse, EdiStandard standard = EdiStandard.Edifact)
        {
            _canParse = canParse;
            _standard = standard;
        }

        public bool CanParse(string text) => _canParse;

        public EdiDocument Parse(string text, IEdiDictionary? dictionary)
        {
            return new EdiDocument(
                _standard, null,
                new List<EdiSegment>
                {
                    new EdiSegment("TST", text, 0, text.Length)
                });
        }
    }

    [Fact]
    public void Dispatch_WithMatchingParser_ReturnsResult()
    {
        var parsers = new List<IEdiParser> { new MockParser(canParse: true) };
        var dispatcher = new EdiParserDispatcher(parsers);
        var result = dispatcher.Parse("UNB+UNOC:3", null);
        Assert.Equal(EdiStandard.Edifact, result.Standard);
        Assert.Single(result.Segments);
    }

    [Fact]
    public void Dispatch_WithNoMatch_ReturnsUnknown()
    {
        var parsers = new List<IEdiParser> { new MockParser(canParse: false) };
        var dispatcher = new EdiParserDispatcher(parsers);
        var result = dispatcher.Parse("random text", null);
        Assert.Equal(EdiStandard.Unknown, result.Standard);
    }

    [Fact]
    public void Dispatch_WithMultipleParsers_UsesFirstMatch()
    {
        var parsers = new List<IEdiParser>
        {
            new MockParser(canParse: false, EdiStandard.X12),
            new MockParser(canParse: true, EdiStandard.Edifact),
            new MockParser(canParse: true, EdiStandard.Vda)
        };
        var dispatcher = new EdiParserDispatcher(parsers);
        var result = dispatcher.Parse("UNB+UNOC:3", null);
        Assert.Equal(EdiStandard.Edifact, result.Standard);
    }

    [Fact]
    public void Dispatch_EmptyText_ReturnsUnknown()
    {
        var parsers = new List<IEdiParser> { new MockParser(canParse: false) };
        var dispatcher = new EdiParserDispatcher(parsers);
        var result = dispatcher.Parse(string.Empty, null);
        Assert.Equal(EdiStandard.Unknown, result.Standard);
    }
}
