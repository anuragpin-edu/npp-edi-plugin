using Edi.Core.Model;
using Xunit;

namespace Edi.Core.Tests.Model;

public class EdiDelimitersTests
{
    [Fact]
    public void EdifactDefaults_HasCorrectValues()
    {
        // Arrange & Act
        var defaults = EdiDelimiters.EdifactDefaults;

        // Assert
        Assert.Equal(':', defaults.ComponentSeparator);
        Assert.Equal('+', defaults.ElementSeparator);
        Assert.Equal('.', defaults.DecimalMark);
        Assert.Equal('?', defaults.ReleaseCharacter);
        Assert.Equal('\'', defaults.SegmentTerminator);
    }

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        char componentSep = '~';
        char elementSep = '^';
        char decimalMark = ',';
        char releaseChar = '!';
        char segmentTerminator = '|';

        // Act
        var delimiters = new EdiDelimiters(componentSep, elementSep, decimalMark, releaseChar, segmentTerminator);

        // Assert
        Assert.Equal(componentSep, delimiters.ComponentSeparator);
        Assert.Equal(elementSep, delimiters.ElementSeparator);
        Assert.Equal(decimalMark, delimiters.DecimalMark);
        Assert.Equal(releaseChar, delimiters.ReleaseCharacter);
        Assert.Equal(segmentTerminator, delimiters.SegmentTerminator);
    }
}
