using FluentAssertions;
using Xunit;
using Edi.X12.Parsing;

namespace Edi.X12.Tests.Parsing
{
    public class X12ParserTests
    {
        [Fact]
        public void Parse_ValidX12_ReturnsDocument()
        {
            string text = "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *260715*1200*U*00401*000000001*0*T*>~GS*PO*SENDER*RECEIVER*20260715*1200*1*X*004010~ST*850*0001~BEG*00*SA*PO-2026-001**20260715~N1*ST*Ship To Name*92*LOCATION1~N3*123 Main Street~N4*Anytown*TX*75001~PO1*1*10*EA*25.00*PE*VP*WIDGET-A~CTT*1~SE*8*0001~GE*1*1~IEA*1*000000001~";
            var parser = new X12Parser();

            var doc = parser.Parse(text, null);

            doc.Should().NotBeNull();
            doc.Segments.Count.Should().Be(12);
            doc.Issues.Should().BeEmpty();
        }
    }
}
