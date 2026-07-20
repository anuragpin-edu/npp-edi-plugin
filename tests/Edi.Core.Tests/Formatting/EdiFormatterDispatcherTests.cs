using Edi.Core.Formatting;
using Xunit;

namespace Edi.Core.Tests.Formatting;

public class EdiFormatterDispatcherTests
{
    /// <summary>
    /// A mock formatter that reports whether it can handle a given text
    /// and returns predictable prettify/minify output.
    /// </summary>
    private class MockFormatter : IEdiFormatter
    {
        private readonly bool _canFormat;
        private readonly string _label;

        public MockFormatter(bool canFormat, string label = "mock")
        {
            _canFormat = canFormat;
            _label = label;
        }

        public bool CanFormat(string text) => _canFormat;
        public string Prettify(string text) => $"[{_label}-prettified]{text}";
        public string Minify(string text) => $"[{_label}-minified]{text}";
    }

    // ── Prettify ─────────────────────────────────────────────────────────

    [Fact]
    public void Prettify_WithMatchingFormatter_ReturnsSuccess()
    {
        var dispatcher = new EdiFormatterDispatcher(new[] { new MockFormatter(true) });

        var result = dispatcher.Prettify("UNB+UNOC:3");

        Assert.True(result.IsSuccess);
        Assert.Contains("[mock-prettified]", result.FormattedText);
    }

    [Fact]
    public void Prettify_WithNoMatchingFormatter_ReturnsFailure()
    {
        var dispatcher = new EdiFormatterDispatcher(new[] { new MockFormatter(false) });

        var result = dispatcher.Prettify("ISA*00");

        Assert.False(result.IsSuccess);
        Assert.Contains("Unsupported", result.ErrorMessage);
    }

    [Fact]
    public void Prettify_WithEmptyText_ReturnsFailure()
    {
        var dispatcher = new EdiFormatterDispatcher(new[] { new MockFormatter(true) });

        var result = dispatcher.Prettify("");

        Assert.False(result.IsSuccess);
        Assert.Contains("No text", result.ErrorMessage);
    }

    [Fact]
    public void Prettify_WithNullText_ReturnsFailure()
    {
        var dispatcher = new EdiFormatterDispatcher(new[] { new MockFormatter(true) });

        var result = dispatcher.Prettify(null!);

        Assert.False(result.IsSuccess);
        Assert.Contains("No text", result.ErrorMessage);
    }

    [Fact]
    public void Prettify_WithMultipleFormatters_UsesFirstMatch()
    {
        var formatters = new IEdiFormatter[]
        {
            new MockFormatter(false, "x12"),
            new MockFormatter(true, "edifact"),
            new MockFormatter(true, "vda")
        };
        var dispatcher = new EdiFormatterDispatcher(formatters);

        var result = dispatcher.Prettify("some text");

        Assert.True(result.IsSuccess);
        Assert.Contains("[edifact-prettified]", result.FormattedText);
    }

    // ── Minify ───────────────────────────────────────────────────────────

    [Fact]
    public void Minify_WithMatchingFormatter_ReturnsSuccess()
    {
        var dispatcher = new EdiFormatterDispatcher(new[] { new MockFormatter(true) });

        var result = dispatcher.Minify("UNB+UNOC:3");

        Assert.True(result.IsSuccess);
        Assert.Contains("[mock-minified]", result.FormattedText);
    }

    [Fact]
    public void Minify_WithNoMatchingFormatter_ReturnsFailure()
    {
        var dispatcher = new EdiFormatterDispatcher(new[] { new MockFormatter(false) });

        var result = dispatcher.Minify("ISA*00");

        Assert.False(result.IsSuccess);
        Assert.Contains("Unsupported", result.ErrorMessage);
    }

    [Fact]
    public void Minify_WithEmptyText_ReturnsFailure()
    {
        var dispatcher = new EdiFormatterDispatcher(new[] { new MockFormatter(true) });

        var result = dispatcher.Minify("");

        Assert.False(result.IsSuccess);
        Assert.Contains("No text", result.ErrorMessage);
    }

    [Fact]
    public void Minify_WithMultipleFormatters_UsesFirstMatch()
    {
        var formatters = new IEdiFormatter[]
        {
            new MockFormatter(false, "x12"),
            new MockFormatter(true, "edifact"),
            new MockFormatter(true, "vda")
        };
        var dispatcher = new EdiFormatterDispatcher(formatters);

        var result = dispatcher.Minify("some text");

        Assert.True(result.IsSuccess);
        Assert.Contains("[edifact-minified]", result.FormattedText);
    }

    // ── FormatResult ─────────────────────────────────────────────────────

    [Fact]
    public void FormatResult_Success_ContainsFormattedText()
    {
        var result = FormatResult.Success("formatted");

        Assert.True(result.IsSuccess);
        Assert.Equal("formatted", result.FormattedText);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void FormatResult_Failure_ContainsErrorMessage()
    {
        var result = FormatResult.Failure("something went wrong");

        Assert.False(result.IsSuccess);
        Assert.Null(result.FormattedText);
        Assert.Equal("something went wrong", result.ErrorMessage);
    }

    // ── No formatters registered ─────────────────────────────────────────

    [Fact]
    public void Prettify_WithNoFormatters_ReturnsFailure()
    {
        var dispatcher = new EdiFormatterDispatcher(new IEdiFormatter[] { });

        var result = dispatcher.Prettify("UNB+UNOC:3");

        Assert.False(result.IsSuccess);
        Assert.Contains("Unsupported", result.ErrorMessage);
    }

    [Fact]
    public void Minify_WithNoFormatters_ReturnsFailure()
    {
        var dispatcher = new EdiFormatterDispatcher(new IEdiFormatter[] { });

        var result = dispatcher.Minify("UNB+UNOC:3");

        Assert.False(result.IsSuccess);
        Assert.Contains("Unsupported", result.ErrorMessage);
    }
}
