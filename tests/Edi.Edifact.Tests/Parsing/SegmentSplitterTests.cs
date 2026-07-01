using Edi.Core.Model;
using Edi.Edifact.Parsing;
using Xunit;

namespace Edi.Edifact.Tests.Parsing;

public class SegmentSplitterTests
{
    private static readonly EdiDelimiters Defaults = EdiDelimiters.EdifactDefaults;

    [Fact]
    public void Split_SimpleSegments_CorrectCount()
    {
        // Arrange
        var text = "BGM+220'DTM+137:20260701:102'";

        // Act
        var result = EdifactSegmentSplitter.Split(text, Defaults, startOffset: 0);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.StartsWith("BGM", result[0].RawText);
        Assert.StartsWith("DTM", result[1].RawText);
    }

    [Fact]
    public void Split_WithReleaseChar_DoesNotSplitOnEscapedTerminator()
    {
        // Arrange — the ? before ' escapes the segment terminator
        var text = "FTX+AAA+++Text with escaped?'quote'NAD+BY'";

        // Act
        var result = EdifactSegmentSplitter.Split(text, Defaults, startOffset: 0);

        // Assert — should be 2 segments: the FTX (with escaped ') and the NAD
        Assert.Equal(2, result.Count);
        Assert.StartsWith("FTX", result[0].RawText);
        Assert.StartsWith("NAD", result[1].RawText);
    }

    [Fact]
    public void Split_WithWhitespace_TrimsAndSkipsEmpty()
    {
        // Arrange — segments separated by newlines
        var text = "BGM+220'\n\nDTM+137:20260701:102'\n";

        // Act
        var result = EdifactSegmentSplitter.Split(text, Defaults, startOffset: 0);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Split_EmptyInput_ReturnsEmpty()
    {
        // Arrange
        var text = "";

        // Act
        var result = EdifactSegmentSplitter.Split(text, Defaults, startOffset: 0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Split_OffsetsAreCorrect()
    {
        // Arrange
        var text = "BGM+220'DTM+137:20260701:102'";
        //          01234567 890123456789012345678
        //          BGM+220' starts at 0, ends at 7 (inclusive of ')
        //          DTM+137:20260701:102' starts at 8, ends at 28

        // Act
        var result = EdifactSegmentSplitter.Split(text, Defaults, startOffset: 0);

        // Assert
        Assert.Equal(0, result[0].StartOffset);
        Assert.True(result[0].EndOffset > result[0].StartOffset);
        Assert.True(result[1].StartOffset > result[0].StartOffset);
        Assert.True(result[1].EndOffset > result[1].StartOffset);
        // Second segment starts after first segment ends
        Assert.True(result[1].StartOffset >= result[0].EndOffset);
    }
}
