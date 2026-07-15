using FluentAssertions;
using Xunit;
using Edi.X12.Detection;

namespace Edi.X12.Tests.Detection
{
    public class X12DelimiterDetectorTests
    {
        [Fact]
        public void IsX12_ValidIsa_ReturnsTrue()
        {
            string text = "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *260715*1200*U*00401*000000001*0*T*>~";
            X12DelimiterDetector.IsX12(text).Should().BeTrue();
        }

        [Fact]
        public void Detect_ValidIsa_ReturnsCorrectDelimiters()
        {
            string text = "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *260715*1200*U*00401*000000001*0*T*>~";
            var delimiters = X12DelimiterDetector.Detect(text);

            delimiters.ElementSeparator.Should().Be('*');
            delimiters.ComponentSeparator.Should().Be('>');
            delimiters.SegmentTerminator.Should().Be('~');
        }
    }
}
