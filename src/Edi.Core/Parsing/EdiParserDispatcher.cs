using System.Collections.Generic;
using System.Linq;
using Edi.Core.Dictionary;
using Edi.Core.Model;

namespace Edi.Core.Parsing
{
    /// <summary>
    /// Dispatches parsing to the first matching parser.
    /// </summary>
    public class EdiParserDispatcher
    {
        private readonly List<IEdiParser> _parsers;

        public EdiParserDispatcher(IEnumerable<IEdiParser> parsers)
        {
            _parsers = parsers?.ToList() ?? new List<IEdiParser>();
        }

        public EdiDocument Parse(string text, IEdiDictionary? dictionary)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new EdiDocument(EdiStandard.Unknown, null, new List<EdiSegment>());
            }

            foreach (var parser in _parsers)
            {
                if (parser.CanParse(text))
                {
                    return parser.Parse(text, dictionary);
                }
            }

            return new EdiDocument(EdiStandard.Unknown, null, new List<EdiSegment>());
        }
    }
}
