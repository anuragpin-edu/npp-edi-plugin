using System.Collections.Generic;
using Edi.Core.Model;
using Xunit;

namespace Edi.Core.Tests.Model;

public class EdiDocumentTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var segments = new List<EdiSegment> { new EdiSegment("TAG", "TAG+1", 0, 5) };
        var issues = new List<EdiValidationIssue> { new EdiValidationIssue("CODE", "MSG", 0, 5, EdiIssueSeverity.Error) };
        var delimiters = EdiDelimiters.EdifactDefaults;

        var doc = new EdiDocument(EdiStandard.Edifact, "D96A", "D96A", segments, issues, delimiters);

        Assert.Equal(EdiStandard.Edifact, doc.Standard);
        Assert.Equal("D96A", doc.Version);
        Assert.Same(segments, doc.Segments);
        Assert.Same(issues, doc.Issues);
        Assert.Same(delimiters, doc.Delimiters);
    }

    [Fact]
    public void Constructor_WithNullIssues_InitializesEmptyList()
    {
        var segments = new List<EdiSegment>();
        var doc = new EdiDocument(EdiStandard.Unknown, null, null, segments);

        Assert.Equal(EdiStandard.Unknown, doc.Standard);
        Assert.Null(doc.Version);
        Assert.Empty(doc.Issues);
    }
}
