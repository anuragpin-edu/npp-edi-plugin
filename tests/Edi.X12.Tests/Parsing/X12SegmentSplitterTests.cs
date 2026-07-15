using Xunit;
using Edi.Core.Model;
using Edi.X12.Parsing;

namespace Edi.X12.Tests.Parsing
{
    public class X12SegmentSplitterTests
    {
        [Fact]
        public void Split_ValidText_ReturnsSegments()
        {
            string text = "ISA*00*~GS*PO*~ST*850*~";
            var delimiters = new EdiDelimiters('>', '*', '\0', '\0', '~');

            var segments = X12SegmentSplitter.Split(text, delimiters, 0);

            Assert.Equal(3, segments.Count);
            Assert.Equal("ISA*00*", segments[0].RawText);
            Assert.Equal("GS*PO*", segments[1].RawText);
            Assert.Equal("ST*850*", segments[2].RawText);
        }
    }
}
