using Edi.Core.Dictionary;
using Edi.Core.Model;
using Xunit;

namespace Edi.Core.Tests.Dictionary
{
    public class EdiVersionNormalizerTests
    {
        [Theory]
        [InlineData("004010", "00401")]
        [InlineData("005010", "00501")]
        [InlineData("004010X092A1", "00401")]
        [InlineData("005010X222A1", "00501")]
        [InlineData("00401", "00401")]
        [InlineData("00501", "00501")]
        [InlineData("ABCDE123", "")]
        [InlineData("123", "")]
        [InlineData(null, "")]
        [InlineData("   ", "")]
        [InlineData("D96A", "")]
        public void Normalize_X12_ReturnsBaseRelease(string? raw, string expected)
        {
            var result = EdiVersionNormalizer.Normalize(EdiStandard.X12, raw);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("D:96A", "D96A")]
        [InlineData("D96A", "D96A")]
        [InlineData("D:01B", "D01B")]
        [InlineData("D01B", "D01B")]
        public void Normalize_Edifact_ReturnsSyntax(string raw, string expected)
        {
            var result = EdiVersionNormalizer.Normalize(EdiStandard.Edifact, raw);
            Assert.Equal(expected, result);
        }
    }
}
