using Edi.Edifact.Formatting;
using Xunit;

namespace Edi.Edifact.Tests.Formatting;

public class FormatterTests
{
    private readonly EdifactFormatter _formatter = new();

    /// <summary>
    /// A compact (minified) EDIFACT message with UNA for use in tests.
    /// </summary>
    private const string MinifiedWithUna =
        "UNA:+.? '" +
        "UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ+260701:1200+00001'" +
        "UNH+1+ORDERS:D:96A:UN'" +
        "BGM+220+PO-2026-00042+9'" +
        "UNT+3+1'" +
        "UNZ+1+00001'";

    /// <summary>
    /// A prettified version (segments on separate lines) for testing minify.
    /// </summary>
    private const string PrettifiedWithUna =
        "UNA:+.? '\n" +
        "UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ+260701:1200+00001'\n" +
        "UNH+1+ORDERS:D:96A:UN'\n" +
        "BGM+220+PO-2026-00042+9'\n" +
        "UNT+3+1'\n" +
        "UNZ+1+00001'";

    /// <summary>
    /// EDIFACT message with release character to test that formatting doesn't break escaping.
    /// </summary>
    private const string MessageWithReleaseChar =
        "UNA:+.? '" +
        "UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ+260701:1200+00001'" +
        "FTX+AAA+++Price is 100?+50'" +
        "UNT+2+1'" +
        "UNZ+1+00001'";

    [Fact]
    public void Prettify_PutsSegmentsOnSeparateLines()
    {
        // Act
        var result = _formatter.Prettify(MinifiedWithUna);

        // Assert — each segment should end with '\n (segment terminator + newline) or be on its own line
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length >= 6,
            $"Expected at least 6 lines (UNA + 5 segments), got {lines.Length}");
    }

    [Fact]
    public void Prettify_PreservesUna()
    {
        // Act
        var result = _formatter.Prettify(MinifiedWithUna);

        // Assert
        Assert.StartsWith("UNA:+.? '", result);
    }

    [Fact]
    public void Minify_RemovesWhitespace()
    {
        // Act
        var result = _formatter.Minify(PrettifiedWithUna);

        // Assert — result should not contain newlines between segments
        // (it may have a trailing newline, but segments should be contiguous)
        var segmentCount = result.Split('\'').Length - 1; // count segment terminators
        Assert.True(segmentCount >= 5, "Should preserve all segments");
        // Verify no newlines between segments
        var afterUna = result.Substring(result.IndexOf("UNB"));
        Assert.DoesNotContain("\n", afterUna.TrimEnd());
    }

    [Fact]
    public void Minify_PreservesUna()
    {
        // Act
        var result = _formatter.Minify(PrettifiedWithUna);

        // Assert
        Assert.StartsWith("UNA:+.? '", result);
    }

    [Fact]
    public void PrettifyThenMinify_ProducesValidOutput()
    {
        // Arrange
        var prettified = _formatter.Prettify(MinifiedWithUna);

        // Act
        var roundTripped = _formatter.Minify(prettified);

        // Assert — the round-tripped version should contain the same segments
        Assert.Contains("UNB+UNOC:3", roundTripped);
        Assert.Contains("UNH+1+ORDERS", roundTripped);
        Assert.Contains("BGM+220", roundTripped);
        Assert.Contains("UNT+3+1", roundTripped);
        Assert.Contains("UNZ+1+00001", roundTripped);
    }

    [Fact]
    public void Prettify_WithReleaseChar_DoesNotBreak()
    {
        // Act
        var result = _formatter.Prettify(MessageWithReleaseChar);

        // Assert — the escaped sequence ?+ should remain intact
        Assert.Contains("?+", result);
        // The FTX segment should be present as a single segment
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var ftxLine = lines.FirstOrDefault(l => l.TrimStart().StartsWith("FTX"));
        Assert.NotNull(ftxLine);
        Assert.Contains("100?+50", ftxLine);
    }
}
