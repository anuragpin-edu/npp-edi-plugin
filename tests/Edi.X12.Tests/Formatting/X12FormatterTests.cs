using Xunit;
using Edi.X12.Formatting;
using System;

namespace Edi.X12.Tests.Formatting
{
    public class X12FormatterTests
    {
        [Fact]
        public void Prettify_MinifiedX12_ReturnsPrettified()
        {
            string text = "ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *260715*1200*U*00401*000000001*0*T*>~GS*PO*SENDER*RECEIVER*20260715*1200*1*X*004010~";
            var formatter = new X12Formatter();

            string pretty = formatter.Prettify(text);

            string expected = $"ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *260715*1200*U*00401*000000001*0*T*>~{Environment.NewLine}GS*PO*SENDER*RECEIVER*20260715*1200*1*X*004010~{Environment.NewLine}";
            Assert.Equal(expected, pretty);
        }
    }
}
