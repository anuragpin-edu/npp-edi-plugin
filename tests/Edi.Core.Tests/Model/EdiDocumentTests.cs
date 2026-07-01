using Edi.Core.Model;
using Xunit;

namespace Edi.Core.Tests.Model;

public class EdiDocumentTests
{
    [Fact]
    public void Constructor_WithSegments_SetsProperties()
    {
        // Arrange
        var segments = new List<EdiSegment>
        {
            new EdiSegment("UNB", "UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ+260701:1200+00001", 0, 55),
            new EdiSegment("UNZ", "UNZ+1+00001", 56, 68)
        };
        var issues = new List<EdiValidationIssue>
        {
            new EdiValidationIssue("TEST001", "Test issue", 0, 10, EdiIssueSeverity.Warning)
        };
        var delimiters = EdiDelimiters.EdifactDefaults;

        // Act
        var doc = new EdiDocument(EdiStandard.Edifact, segments, issues, delimiters);

        // Assert
        Assert.Equal(EdiStandard.Edifact, doc.Standard);
        Assert.Equal(2, doc.Segments.Count);
        Assert.Single(doc.Issues);
        Assert.NotNull(doc.Delimiters);
        Assert.Equal(':', doc.Delimiters!.ComponentSeparator);
    }

    [Fact]
    public void Constructor_WithNullIssues_CreatesEmptyList()
    {
        // Arrange
        var segments = new List<EdiSegment>
        {
            new EdiSegment("UNB", "UNB+UNOC:3", 0, 10)
        };

        // Act
        var doc = new EdiDocument(EdiStandard.Edifact, segments, issues: null);

        // Assert
        Assert.NotNull(doc.Issues);
        Assert.Empty(doc.Issues);
    }

    [Fact]
    public void Constructor_WithEmptySegments_Works()
    {
        // Arrange
        var segments = new List<EdiSegment>();

        // Act
        var doc = new EdiDocument(EdiStandard.Unknown, segments);

        // Assert
        Assert.Equal(EdiStandard.Unknown, doc.Standard);
        Assert.Empty(doc.Segments);
        Assert.Empty(doc.Issues);
        Assert.Null(doc.Delimiters);
    }
}
