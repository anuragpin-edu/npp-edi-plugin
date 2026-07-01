using Edi.Core.Model;
using Edi.Edifact.Detection;
using Xunit;

namespace Edi.Edifact.Tests.Detection;

public class DelimiterDetectionTests
{
    [Fact]
    public void Detect_WithStandardUna_ReturnsDefaults()
    {
        // Arrange
        var text = "UNA:+.? 'UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ+260701:1200+00001'";

        // Act
        var delimiters = EdifactDelimiterDetector.Detect(text);

        // Assert
        Assert.Equal(':', delimiters.ComponentSeparator);
        Assert.Equal('+', delimiters.ElementSeparator);
        Assert.Equal('.', delimiters.DecimalMark);
        Assert.Equal('?', delimiters.ReleaseCharacter);
        Assert.Equal('\'', delimiters.SegmentTerminator);
    }

    [Fact]
    public void Detect_WithCustomUna_ExtractsCustomDelimiters()
    {
        // Arrange — UNA~^.?|' means component=~, element=^, decimal=., release=?, segment=|
        // The 6th char after UNA (position 8) is reserved; segment terminator is at position 8
        // UNA format: UNA<comp><elem><dec><release><reserved><seg>
        var text = "UNA~^.? |UNB^UNOC~3^SENDER~ZZ^RECEIVER~ZZ^260701~1200^00001|";

        // Act
        var delimiters = EdifactDelimiterDetector.Detect(text);

        // Assert
        Assert.Equal('~', delimiters.ComponentSeparator);
        Assert.Equal('^', delimiters.ElementSeparator);
        Assert.Equal('.', delimiters.DecimalMark);
        Assert.Equal('?', delimiters.ReleaseCharacter);
        Assert.Equal('|', delimiters.SegmentTerminator);
    }

    [Fact]
    public void Detect_WithoutUna_ReturnsDefaults()
    {
        // Arrange — no UNA segment, starts directly with UNB
        var text = "UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ+260701:1200+00001'";

        // Act
        var delimiters = EdifactDelimiterDetector.Detect(text);

        // Assert
        Assert.Equal(':', delimiters.ComponentSeparator);
        Assert.Equal('+', delimiters.ElementSeparator);
        Assert.Equal('.', delimiters.DecimalMark);
        Assert.Equal('?', delimiters.ReleaseCharacter);
        Assert.Equal('\'', delimiters.SegmentTerminator);
    }

    [Fact]
    public void IsEdifact_WithUna_ReturnsTrue()
    {
        // Arrange
        var text = "UNA:+.? 'UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ'";

        // Act & Assert
        Assert.True(EdifactDelimiterDetector.IsEdifact(text));
    }

    [Fact]
    public void IsEdifact_WithUnb_ReturnsTrue()
    {
        // Arrange — no UNA, starts with UNB
        var text = "UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ+260701:1200+00001'";

        // Act & Assert
        Assert.True(EdifactDelimiterDetector.IsEdifact(text));
    }

    [Fact]
    public void IsEdifact_WithRandomText_ReturnsFalse()
    {
        // Arrange
        var text = "Hello, this is just some random text without any EDI content.";

        // Act & Assert
        Assert.False(EdifactDelimiterDetector.IsEdifact(text));
    }

    [Fact]
    public void IsEdifact_WithIsa_ReturnsFalse()
    {
        // Arrange — ISA is X12, not EDIFACT
        var text = "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *260701*1200*U*00401*000000001*0*P*>~";

        // Act & Assert
        Assert.False(EdifactDelimiterDetector.IsEdifact(text));
    }

    [Fact]
    public void GetDataStartOffset_WithUna_Returns9()
    {
        // Arrange — UNA is 3 chars + 6 delimiter chars = 9 characters
        var text = "UNA:+.? 'UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ'";

        // Act
        var offset = EdifactDelimiterDetector.GetDataStartOffset(text);

        // Assert
        Assert.Equal(9, offset);
    }

    [Fact]
    public void GetDataStartOffset_WithoutUna_Returns0()
    {
        // Arrange
        var text = "UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ'";

        // Act
        var offset = EdifactDelimiterDetector.GetDataStartOffset(text);

        // Assert
        Assert.Equal(0, offset);
    }
}
