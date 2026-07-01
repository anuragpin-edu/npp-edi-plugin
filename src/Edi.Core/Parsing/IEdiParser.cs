using Edi.Core.Dictionary;
using Edi.Core.Model;

namespace Edi.Core.Parsing
{
    /// <summary>
    /// Defines a parser capable of reading a specific EDI standard.
    /// </summary>
    public interface IEdiParser
    {
        bool CanParse(string text);
        EdiDocument Parse(string text, IEdiDictionary? dictionary);
    }
}
