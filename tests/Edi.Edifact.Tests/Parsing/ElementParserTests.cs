using Edi.Core.Dictionary;
using Edi.Core.Model;
using Edi.Edifact.Parsing;
using Xunit;

namespace Edi.Edifact.Tests.Parsing;

public class ElementParserTests
{
    private static readonly EdiDelimiters Defaults = EdiDelimiters.EdifactDefaults;

    [Fact]
    public void Parse_SimpleElements_CorrectCount()
    {
        // Arrange — segment body after tag: "220+PO12345+9" has 3 elements
        var segmentBody = "220+PO12345+9";

        // Act
        var result = EdifactElementParser.ParseElements(
            segmentBody, Defaults, dictionary: null, EdiStandard.Edifact, "BGM");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("220", result[0].RawValue);
        Assert.Equal("PO12345", result[1].RawValue);
        Assert.Equal("9", result[2].RawValue);
    }

    [Fact]
    public void Parse_CompositeElement_HasComponents()
    {
        // Arrange — "UNOC:3" is a composite element with 2 components
        var segmentBody = "UNOC:3";

        // Act
        var result = EdifactElementParser.ParseElements(
            segmentBody, Defaults, dictionary: null, EdiStandard.Edifact, "UNB");

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Components.Count);
        Assert.Equal("UNOC", result[0].Components[0].RawValue);
        Assert.Equal("3", result[0].Components[1].RawValue);
    }

    [Fact]
    public void Parse_ReleaseCharInValue_PreservesLiteral()
    {
        // Arrange — ?+ is an escaped +, so "A?+B" should be treated as a single value "A+B"
        var segmentBody = "A?+B";

        // Act
        var result = EdifactElementParser.ParseElements(
            segmentBody, Defaults, dictionary: null, EdiStandard.Edifact, "FTX");

        // Assert
        Assert.Single(result);
        Assert.Contains("+", result[0].RawValue);
    }

    [Fact]
    public void Parse_EmptyTrailingElements_Included()
    {
        // Arrange — "A+B+" has 3 elements, the last one is empty
        var segmentBody = "A+B+";

        // Act
        var result = EdifactElementParser.ParseElements(
            segmentBody, Defaults, dictionary: null, EdiStandard.Edifact, "TST");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("", result[2].RawValue);
    }

    [Fact]
    public void Parse_DoubleReleaseChar_BecomesLiteral()
    {
        // Arrange — ?? should become a single ? in the value
        var segmentBody = "A??B";

        // Act
        var result = EdifactElementParser.ParseElements(
            segmentBody, Defaults, dictionary: null, EdiStandard.Edifact, "FTX");

        // Assert
        Assert.Single(result);
        Assert.Contains("?", result[0].RawValue);
    }
}
