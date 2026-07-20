using Edi.Core.Dictionary;
using Edi.Core.Model;
using Xunit;

namespace Edi.Core.Tests.Dictionary;

public class NullEdiDictionaryTests
{
    private readonly NullEdiDictionary _dictionary = new();

    [Fact]
    public void GetSegmentLabel_ReturnsNull()
    {
        // Act
        var result = _dictionary.GetSegmentLabel(EdiStandard.Edifact, "D96A", "UNB");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetElementLabel_ReturnsNull()
    {
        // Act
        var result = _dictionary.GetElementLabel(EdiStandard.Edifact, "D96A", "UNB", 1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetComponentLabel_ReturnsNull()
    {
        // Act
        var result = _dictionary.GetComponentLabel(EdiStandard.Edifact, "D96A", "UNB", 1, 0);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetQualifierLabel_ReturnsNull()
    {
        // Act
        var result = _dictionary.GetQualifierLabel(EdiStandard.Edifact, "D96A", "NAD", 1, "BY");

        // Assert
        Assert.Null(result);
    }
}
