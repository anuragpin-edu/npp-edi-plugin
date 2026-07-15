using Edi.Core.Model;
using Edi.Dictionaries;
using Xunit;

namespace Edi.Dictionaries.Tests;

public class JsonEdiDictionaryTests
{
    private readonly JsonEdiDictionary _dictionary = new();

    [Fact]
    public void GetSegmentLabel_Unb_ReturnsInterchangeHeader()
    {
        // Act
        var label = _dictionary.GetSegmentLabel(EdiStandard.Edifact, "D96A", "UNB");

        // Assert
        Assert.NotNull(label);
        Assert.Contains("Interchange", label, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetSegmentLabel_Unknown_ReturnsNull()
    {
        // Act
        var label = _dictionary.GetSegmentLabel(EdiStandard.Edifact, "D96A", "ZZZ");

        // Assert
        Assert.Null(label);
    }

    [Fact]
    public void GetElementLabel_BgmPosition1_ReturnsDocumentMessageName()
    {
        // Act
        var label = _dictionary.GetElementLabel(EdiStandard.Edifact, "D96A", "BGM", 1);

        // Assert
        Assert.NotNull(label);
        Assert.Contains("Document", label, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetQualifierLabel_NadBy_ReturnsBuyer()
    {
        // Act
        var label = _dictionary.GetQualifierLabel(EdiStandard.Edifact, "D96A", "NAD", 1, "BY");

        // Assert
        Assert.NotNull(label);
        Assert.Contains("Buyer", label, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetQualifierLabel_UnknownValue_ReturnsNull()
    {
        // Act
        var label = _dictionary.GetQualifierLabel(EdiStandard.Edifact, "D96A", "NAD", 1, "ZZZZZ");

        // Assert
        Assert.Null(label);
    }

    [Fact]
    public void GetSegmentLabel_X12Standard_ReturnsNull()
    {
        // Act — the JSON dictionary only covers EDIFACT in Phase 1
        var label = _dictionary.GetSegmentLabel(EdiStandard.X12, "00401", "ISA");

        // Assert
        Assert.Null(label);
    }

    [Fact]
    public void GetComponentLabel_UnbElement1Component0_ReturnsSyntaxIdentifierCode()
    {
        // Act
        var label = _dictionary.GetComponentLabel(EdiStandard.Edifact, "D96A", "UNB", 1, 0);

        // Assert
        Assert.NotNull(label);
        Assert.Contains("Syntax", label, StringComparison.OrdinalIgnoreCase);
    }
}
