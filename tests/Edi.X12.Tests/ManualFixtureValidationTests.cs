using System.IO;
using System.Linq;
using Edi.Core.Model;
using Edi.Dictionaries;
using Edi.X12.Parsing;
using Edi.X12.Validation;
using Xunit;

namespace Edi.X12.Tests
{
    public class ManualFixtureValidationTests
    {
        private readonly JsonEdiDictionary _dictionary;
        private readonly X12Parser _parser;
        private readonly string _samplesDir;

        public ManualFixtureValidationTests()
        {
            _dictionary = new JsonEdiDictionary();
            _parser = new X12Parser();
            _samplesDir = Path.GetFullPath(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "ManualSamples"));
        }

        [Fact]
        public void Parse_004010_Success()
        {
            var text = File.ReadAllText(Path.Combine(_samplesDir, "x12_004010_850.edi"));
            var doc = _parser.Parse(text, _dictionary);
            Assert.Equal(EdiStandard.X12, doc.Standard);
            Assert.Empty(doc.Issues);
            Assert.Equal("00401", doc.DictionaryRelease);
            var beg = doc.Segments.First(s => s.Tag == "BEG");
            Assert.Contains("Beginning Segment", beg.Label ?? "", System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Parse_005010_Success()
        {
            var text = File.ReadAllText(Path.Combine(_samplesDir, "x12_005010_850.edi"));
            var doc = _parser.Parse(text, _dictionary);
            Assert.Equal(EdiStandard.X12, doc.Standard);
            Assert.Empty(doc.Issues);
            Assert.Equal("00501", doc.DictionaryRelease);
        }

        [Fact]
        public void Parse_005010X222A1_Success()
        {
            var text = File.ReadAllText(Path.Combine(_samplesDir, "x12_005010X222A1_837.edi"));
            var doc = _parser.Parse(text, _dictionary);
            Assert.Equal(EdiStandard.X12, doc.Standard);
            Assert.Empty(doc.Issues);
            Assert.Equal("005010X222A1", doc.Version);
            Assert.Equal("00501", doc.DictionaryRelease); // Uses base release
        }

        [Fact]
        public void Parse_CustomDelimiters_Success()
        {
            var text = File.ReadAllText(Path.Combine(_samplesDir, "x12_custom_delimiters.edi"));
            var doc = _parser.Parse(text, _dictionary);
            Assert.Equal(EdiStandard.X12, doc.Standard);
            Assert.Empty(doc.Issues);
            Assert.Equal('|', doc.Delimiters?.ElementSeparator);
            Assert.Equal('^', doc.Delimiters?.ComponentSeparator);
            Assert.Equal('\n', doc.Delimiters?.SegmentTerminator);
            Assert.Equal("00501", doc.DictionaryRelease);
        }

        [Fact]
        public void Parse_CrlfFormatted_Success()
        {
            var text = File.ReadAllText(Path.Combine(_samplesDir, "x12_crlf_formatted.edi"));
            var doc = _parser.Parse(text, _dictionary);
            Assert.Equal(EdiStandard.X12, doc.Standard);
            Assert.Empty(doc.Issues);
        }

        [Fact]
        public void Parse_00200_Unsupported_StructuralSuccess()
        {
            var text = File.ReadAllText(Path.Combine(_samplesDir, "x12_unsupported.edi"));
            var doc = _parser.Parse(text, _dictionary);
            Assert.Equal(EdiStandard.X12, doc.Standard);
            Assert.Equal("002000", doc.Version);
            Assert.Equal("00200", doc.DictionaryRelease); // Not found
            var beg = doc.Segments.First(s => s.Tag == "BEG");
            Assert.Null(beg.Label); // No semantic enrichment
        }

        [Fact]
        public void Parse_MalformedIsa_IssuesDetected()
        {
            var text = File.ReadAllText(Path.Combine(_samplesDir, "x12_malformed_isa.edi"));
            var doc = _parser.Parse(text, _dictionary);
            Assert.NotEmpty(doc.Issues);
            Assert.Contains(doc.Issues, i => i.Code.StartsWith("X12_MISSING_"));
        }

        [Fact]
        public void Parse_CountMismatch_IssuesDetected()
        {
            var text = File.ReadAllText(Path.Combine(_samplesDir, "x12_count_mismatch.edi"));
            var doc = _parser.Parse(text, _dictionary);
            Assert.NotEmpty(doc.Issues);
            // Verify that either ST/SE or ISA/IEA counts were flagged
            Assert.Contains(doc.Issues, i => i.Message.Contains("count") || i.Message.Contains("mismatch"));
        }
    }
}
