using System.Linq;
using Edi.Core.Model;
using Edi.Dictionaries;
using Edi.Edifact.Parsing;
using Xunit;

namespace Edi.Edifact.Tests
{
    public class EdifactIntegrationTests
    {
        private const string SampleOrders = "UNA:+.? 'UNB+UNOA:1+SENDER+RECEIVER+071101:1307+00000001'UNH+1+ORDERS:D:96A:UN'BGM+220:::+PO12345+9'UNT+3+1'UNZ+1+00000001'";

        [Fact]
        public void Parse_WithD96ADictionary_SemanticEnrichmentIsApplied()
        {
            var dictionary = new JsonEdiDictionary();
            var parser = new EdifactParser();

            var doc = parser.Parse(SampleOrders, dictionary);

            Assert.Equal(EdiStandard.Edifact, doc.Standard);
            Assert.Equal("D:96A", doc.Version);
            Assert.Equal("D96A", doc.DictionaryRelease);

            var bgmSegment = doc.Segments.FirstOrDefault(s => s.Tag == "BGM");
            Assert.NotNull(bgmSegment);
            Assert.Contains("BEGINNING OF MESSAGE", bgmSegment.Label ?? "", System.StringComparison.OrdinalIgnoreCase);

            var funcCodeElement = bgmSegment.Elements.FirstOrDefault(e => e.Position == 3);
            Assert.NotNull(funcCodeElement);
            Assert.Contains("MESSAGE FUNCTION", funcCodeElement.Label ?? "", System.StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Original", funcCodeElement.ValueDescription ?? "", System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Parse_WithoutDictionary_NoSemanticEnrichment()
        {
            var parser = new EdifactParser();
            var doc = parser.Parse(SampleOrders, null);

            var bgmSegment = doc.Segments.FirstOrDefault(s => s.Tag == "BGM");
            Assert.NotNull(bgmSegment);
            Assert.Null(bgmSegment.Label);

            var funcCodeElement = bgmSegment.Elements.FirstOrDefault(e => e.Position == 3);
            Assert.NotNull(funcCodeElement);
            Assert.Null(funcCodeElement.Label);
            Assert.Null(funcCodeElement.ValueDescription);
        }

        [Fact]
        public void Parse_UnknownQualifier_FallbackGracefully()
        {
            var dictionary = new JsonEdiDictionary();
            var parser = new EdifactParser();

            // BGM element 3 is 999 instead of 9
            var unknownCodeSample = "UNA:+.? 'UNB+UNOA:1+SENDER+RECEIVER+071101:1307+00000001'UNH+1+ORDERS:D:96A:UN'BGM+220:::+PO12345+999'UNT+3+1'UNZ+1+00000001'";
            var doc = parser.Parse(unknownCodeSample, dictionary);

            var bgmSegment = doc.Segments.FirstOrDefault(s => s.Tag == "BGM");
            Assert.NotNull(bgmSegment);

            var funcCodeElement = bgmSegment.Elements.FirstOrDefault(e => e.Position == 3);
            Assert.NotNull(funcCodeElement);
            Assert.Contains("MESSAGE FUNCTION", funcCodeElement.Label ?? "", System.StringComparison.OrdinalIgnoreCase);
            Assert.Null(funcCodeElement.ValueDescription); // Should not find a description for 999
        }
    }
}
