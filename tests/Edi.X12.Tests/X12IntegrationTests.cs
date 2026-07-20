using System.Linq;
using Edi.Core.Model;
using Edi.Dictionaries;
using Edi.X12.Parsing;
using Xunit;

namespace Edi.X12.Tests
{
    public class X12IntegrationTests
    {
        private const string Sample850_00401 = @"ISA*00*          *00*          *ZZ*SENDER1        *ZZ*RECEIVER1      *071216*1406*U*00401*000000263*0*T*:~GS*PO*SENDER1*RECEIVER1*20071216*1406*263*X*004010~ST*850*0001~BEG*00*SA*12345**20071216~SE*4*0001~GE*1*263~IEA*1*000000263~";
        private const string Sample850_00501 = @"ISA*00*          *00*          *ZZ*SENDER1        *ZZ*RECEIVER1      *071216*1406*U*00501*000000263*0*T*:~GS*PO*SENDER1*RECEIVER1*20071216*1406*263*X*005010~ST*850*0001~BEG*00*SA*12345**20071216~SE*4*0001~GE*1*263~IEA*1*000000263~";
        private const string Sample837_00501_HIPAA = @"ISA*00*          *00*          *ZZ*SENDER1        *ZZ*RECEIVER1      *071216*1406*U*00501*000000263*0*T*:~GS*HC*SENDER1*RECEIVER1*20071216*1406*263*X*005010X222A1~ST*837*0001~BHT*0019*00*12345*20071216*1406~SE*4*0001~GE*1*263~IEA*1*000000263~";
        private const string Sample850_UnknownVersion = @"ISA*00*          *00*          *ZZ*SENDER1        *ZZ*RECEIVER1      *071216*1406*U*99999*000000263*0*T*:~GS*PO*SENDER1*RECEIVER1*20071216*1406*263*X*ABCDE~ST*850*0001~BEG*00*SA*12345**20071216~SE*4*0001~GE*1*263~IEA*1*000000263~";

        [Fact]
        public void Parse_With00401Dictionary_SemanticEnrichmentIsApplied()
        {
            var dictionary = new JsonEdiDictionary();
            var parser = new X12Parser();

            var doc = parser.Parse(Sample850_00401, dictionary);

            Assert.Equal(EdiStandard.X12, doc.Standard);
            Assert.Equal("004010", doc.Version);
            Assert.Equal("00401", doc.DictionaryRelease);

            var begSegment = doc.Segments.FirstOrDefault(s => s.Tag == "BEG");
            Assert.NotNull(begSegment);
            Assert.Contains("Beginning Segment", begSegment.Label ?? "", System.StringComparison.OrdinalIgnoreCase);

            var typeCodeElement = begSegment.Elements.FirstOrDefault(e => e.Position == 2);
            Assert.NotNull(typeCodeElement);
            Assert.Equal("SA", typeCodeElement.RawValue);
            
            Assert.Contains("Purchase Order Type", typeCodeElement.Label ?? "", System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Parse_With00501Dictionary_SemanticEnrichmentIsApplied()
        {
            var dictionary = new JsonEdiDictionary();
            var parser = new X12Parser();

            var doc = parser.Parse(Sample850_00501, dictionary);

            Assert.Equal(EdiStandard.X12, doc.Standard);
            Assert.Equal("005010", doc.Version);
            Assert.Equal("00501", doc.DictionaryRelease);

            var begSegment = doc.Segments.FirstOrDefault(s => s.Tag == "BEG");
            Assert.NotNull(begSegment);
            Assert.Contains("Beginning Segment", begSegment.Label ?? "", System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Parse_WithHIPAAVersion_Uses00501DictionaryFallback()
        {
            var dictionary = new JsonEdiDictionary();
            var parser = new X12Parser();

            var doc = parser.Parse(Sample837_00501_HIPAA, dictionary);

            Assert.Equal(EdiStandard.X12, doc.Standard);
            Assert.Equal("005010X222A1", doc.Version);
            Assert.Equal("00501", doc.DictionaryRelease);

            var bhtSegment = doc.Segments.FirstOrDefault(s => s.Tag == "BHT");
            Assert.NotNull(bhtSegment);
            Assert.Contains("Beginning of Hierarchical", bhtSegment.Label ?? "", System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Parse_WithoutDictionary_NoSemanticEnrichment()
        {
            var parser = new X12Parser();
            var doc = parser.Parse(Sample850_00401, null);

            var begSegment = doc.Segments.FirstOrDefault(s => s.Tag == "BEG");
            Assert.NotNull(begSegment);
            Assert.Null(begSegment.Label);

            var typeCodeElement = begSegment.Elements.FirstOrDefault(e => e.Position == 2);
            Assert.NotNull(typeCodeElement);
            Assert.Null(typeCodeElement.Label);
            Assert.Null(typeCodeElement.ValueDescription);
        }

        [Fact]
        public void Parse_UnsupportedVersion_FallbackGracefully()
        {
            var dictionary = new JsonEdiDictionary();
            var parser = new X12Parser();

            var doc = parser.Parse(Sample850_UnknownVersion, dictionary);

            Assert.Equal(EdiStandard.X12, doc.Standard);
            Assert.Equal("ABCDE", doc.Version); // Note GS08 is ABCDE
            Assert.Equal("", doc.DictionaryRelease); // Our regex normalizer returns empty for ABCDE

            var begSegment = doc.Segments.FirstOrDefault(s => s.Tag == "BEG");
            Assert.NotNull(begSegment);
            Assert.Null(begSegment.Label);
        }
    }
}
