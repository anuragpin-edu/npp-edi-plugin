using Edi.Core.Model;
using Edi.Dictionaries;
using Xunit;

namespace Edi.Dictionaries.Tests;

public class JsonEdiDictionaryTests
{
    private readonly JsonEdiDictionary _dictionary = new();

    [Fact]
    public void GetSegmentLabel_Bgm_ReturnsBeginningOfMessage()
    {
        // Act
        var label = _dictionary.GetSegmentLabel(EdiStandard.Edifact, "D96A", "BGM");

        // Assert
        Assert.NotNull(label);
        Assert.Contains("BEGINNING OF MESSAGE", label, StringComparison.OrdinalIgnoreCase);
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
        Assert.Contains("DOCUMENT/MESSAGE NAME", label, StringComparison.OrdinalIgnoreCase);
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
        // Act - the JSON dictionary might not have ISA if not loaded yet
        var label = _dictionary.GetSegmentLabel(EdiStandard.X12, "00401", "ISA");

        // Assert
        Assert.Null(label);
    }

    [Fact]
    public void GetComponentLabel_BgmElement1Component1_ReturnsDocumentNameCode()
    {
        // Act
        var label = _dictionary.GetComponentLabel(EdiStandard.Edifact, "D96A", "BGM", 1, 1);

        // Assert
        Assert.NotNull(label);
        Assert.Contains("Document/message name, coded", label, StringComparison.OrdinalIgnoreCase);
    }
}
