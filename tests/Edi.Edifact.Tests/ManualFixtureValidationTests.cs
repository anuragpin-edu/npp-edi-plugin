using System.IO;
using System.Linq;
using Edi.Core.Model;
using Edi.Dictionaries;
using Edi.Edifact.Parsing;
using Xunit;

namespace Edi.Edifact.Tests
{
    public class ManualFixtureValidationTests
    {
        private readonly JsonEdiDictionary _dictionary;
        private readonly EdifactParser _parser;
        private readonly string _samplesDir;

        public ManualFixtureValidationTests()
        {
            _dictionary = new JsonEdiDictionary();
            _parser = new EdifactParser();
            _samplesDir = Path.GetFullPath(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "ManualSamples"));
        }

        [Fact]
        public void Parse_D96A_WithUna_Success()
        {
            var text = File.ReadAllText(Path.Combine(_samplesDir, "edifact_d96a_orders_with_una.edi"));
            var doc = _parser.Parse(text, _dictionary);
            Assert.Equal(EdiStandard.Edifact, doc.Standard);
            Assert.Empty(doc.Issues);
            var bgm = doc.Segments.First(s => s.Tag == "BGM");
            Assert.Contains("BEGINNING OF MESSAGE", bgm.Label ?? "", System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Parse_D96A_NoUna_Success()
        {
            var text = File.ReadAllText(Path.Combine(_samplesDir, "edifact_d96a_orders_no_una.edi"));
            var doc = _parser.Parse(text, _dictionary);
            Assert.Equal(EdiStandard.Edifact, doc.Standard);
            Assert.Empty(doc.Issues);
        }

        [Fact]
        public void Parse_D96A_Escaped_Success()
        {
            var text = File.ReadAllText(Path.Combine(_samplesDir, "edifact_d96a_escaped.edi"));
            var doc = _parser.Parse(text, _dictionary);
            Assert.Equal(EdiStandard.Edifact, doc.Standard);
            Assert.Empty(doc.Issues);
            var bgm = doc.Segments.First(s => s.Tag == "BGM");
            Assert.Equal("10?+01", bgm.Elements[1].RawValue);
        }

        [Fact]
        public void Parse_D18A_Unsupported_StructuralSuccess()
        {
            var text = File.ReadAllText(Path.Combine(_samplesDir, "edifact_d18a_unsupported.edi"));
            var doc = _parser.Parse(text, _dictionary);
            Assert.Equal(EdiStandard.Edifact, doc.Standard);
            Assert.Equal("D:18A", doc.Version);
            Assert.Equal("D18A", doc.DictionaryRelease); // Not found
            var bgm = doc.Segments.First(s => s.Tag == "BGM");
            Assert.Null(bgm.Label); // No semantic enrichment
        }

        [Fact]
        public void Parse_MalformedEnvelope_IssuesDetected()
        {
            var text = File.ReadAllText(Path.Combine(_samplesDir, "edifact_malformed_envelope.edi"));
            var doc = _parser.Parse(text, _dictionary);
            Assert.NotEmpty(doc.Issues);
        }
    }
}
