using Edi.Core.Model;
using Edi.Edifact.Parsing;
using Xunit;

namespace Edi.Edifact.Tests.Parsing;

public class EdifactParserTests
{
    /// <summary>
    /// Valid EDIFACT ORDERS message with UNA service advice string.
    /// </summary>
    private const string ValidOrdersWithUna =
        "UNA:+.? '" +
        "UNB+UNOC:3+SENDER001:ZZ+RECEIVER001:ZZ+260701:1200+00000000000001'" +
        "UNH+1+ORDERS:D:96A:UN'" +
        "BGM+220+PO-2026-00042+9'" +
        "DTM+137:20260701:102'" +
        "DTM+2:20260715:102'" +
        "NAD+BY+BUYERID001::91'" +
        "NAD+SE+SELLERID001::91'" +
        "LIN+1++WIDGET-A100:SA'" +
        "QTY+21:500:PCE'" +
        "LIN+2++GADGET-B200:SA'" +
        "QTY+21:250:PCE'" +
        "UNS+S'" +
        "UNT+12+1'" +
        "UNZ+1+00000000000001'";

    /// <summary>
    /// Valid EDIFACT message without UNA — starts with UNB using default delimiters.
    /// </summary>
    private const string ValidOrdersWithoutUna =
        "UNB+UNOC:3+SENDER001:ZZ+RECEIVER001:ZZ+260701:1200+00000000000001'" +
        "UNH+1+ORDERS:D:96A:UN'" +
        "BGM+220+PO-2026-00042+9'" +
        "UNT+3+1'" +
        "UNZ+1+00000000000001'";

    /// <summary>
    /// EDIFACT message containing release characters to test escaping.
    /// </summary>
    private const string MessageWithReleaseChars =
        "UNA:+.? '" +
        "UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ+260701:1200+00001'" +
        "UNH+1+ORDERS:D:96A:UN'" +
        "FTX+AAA+++Price is 100?+50 per unit'" +
        "UNT+3+1'" +
        "UNZ+1+00001'";

    private readonly EdifactParser _parser = new();

    [Fact]
    public void CanParse_Edifact_ReturnsTrue()
    {
        // Act & Assert
        Assert.True(_parser.CanParse(ValidOrdersWithUna));
    }

    [Fact]
    public void CanParse_NonEdifact_ReturnsFalse()
    {
        // Arrange
        var nonEdifact = "ISA*00*          *00*          *ZZ*SENDER*ZZ*RECEIVER*260701*1200*U*00401*000000001*0*P*>~";

        // Act & Assert
        Assert.False(_parser.CanParse(nonEdifact));
    }

    [Fact]
    public void Parse_ValidOrders_StandardIsEdifact()
    {
        // Act
        var doc = _parser.Parse(ValidOrdersWithUna, dictionary: null);

        // Assert
        Assert.Equal(EdiStandard.Edifact, doc.Standard);
    }

    [Fact]
    public void Parse_ValidOrders_Has14Segments()
    {
        // Act — UNB, UNH, BGM, DTM, DTM, NAD, NAD, LIN, QTY, LIN, QTY, UNS, UNT, UNZ = 14
        var doc = _parser.Parse(ValidOrdersWithUna, dictionary: null);

        // Assert
        Assert.Equal(14, doc.Segments.Count);
    }

    [Fact]
    public void Parse_ValidOrders_FirstSegmentIsUnb()
    {
        // Act
        var doc = _parser.Parse(ValidOrdersWithUna, dictionary: null);

        // Assert
        Assert.Equal("UNB", doc.Segments[0].Tag);
    }

    [Fact]
    public void Parse_ValidOrders_SegmentOffsetsArePositive()
    {
        // Act
        var doc = _parser.Parse(ValidOrdersWithUna, dictionary: null);

        // Assert
        foreach (var segment in doc.Segments)
        {
            Assert.True(segment.StartOffset >= 0,
                $"Segment {segment.Tag} has negative StartOffset: {segment.StartOffset}");
            Assert.True(segment.EndOffset > segment.StartOffset,
                $"Segment {segment.Tag} has EndOffset ({segment.EndOffset}) <= StartOffset ({segment.StartOffset})");
        }
    }

    [Fact]
    public void Parse_WithoutUna_UsesDefaults()
    {
        // Act
        var doc = _parser.Parse(ValidOrdersWithoutUna, dictionary: null);

        // Assert
        Assert.Equal(EdiStandard.Edifact, doc.Standard);
        Assert.True(doc.Segments.Count > 0);
        Assert.Equal("UNB", doc.Segments[0].Tag);
    }

    [Fact]
    public void Parse_WithReleaseChars_HandlesEscaping()
    {
        // Act
        var doc = _parser.Parse(MessageWithReleaseChars, dictionary: null);

        // Assert
        Assert.Equal(EdiStandard.Edifact, doc.Standard);
        Assert.True(doc.Segments.Count > 0);

        // Find the FTX segment
        var ftx = doc.Segments.FirstOrDefault(s => s.Tag == "FTX");
        Assert.NotNull(ftx);
        // The raw text should contain the release character sequence
        Assert.Contains("?+", ftx!.RawText);
    }
}
