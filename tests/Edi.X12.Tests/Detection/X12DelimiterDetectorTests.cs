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
            Assert.True(X12DelimiterDetector.IsX12(text));
        }

        [Fact]
        public void Detect_ValidIsa_ReturnsCorrectDelimiters()
        {
            string text = "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *260715*1200*U*00401*000000001*0*T*>~";
            var delimiters = X12DelimiterDetector.Detect(text);

            Assert.Equal('*', delimiters.ElementSeparator);
            Assert.Equal('>', delimiters.ComponentSeparator);
            Assert.Equal('~', delimiters.SegmentTerminator);
        }
    }
}
