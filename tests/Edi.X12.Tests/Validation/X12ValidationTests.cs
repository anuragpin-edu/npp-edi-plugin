using FluentAssertions;
using Xunit;
using Edi.X12.Parsing;

namespace Edi.X12.Tests.Validation
{
    public class X12ValidationTests
    {
        [Fact]
        public void Validate_MissingIEA_ReturnsError()
        {
            string text = "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *260715*1200*U*00401*000000001*0*T*>~GS*PO*SENDER*RECEIVER*20260715*1200*1*X*004010~ST*850*0001~BEG*00*SA*PO-2026-001**20260715~SE*2*0001~GE*1*1~";
            var parser = new X12Parser();

            var doc = parser.Parse(text, null);

            doc.ValidationIssues.Should().NotBeNull();
            doc.ValidationIssues.Should().Contain(i => i.Code == "X12_MISSING_IEA");
        }
    }
}
