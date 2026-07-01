using System.Collections.Generic;

namespace Edi.Core.Model
{
    /// <summary>
    /// Represents a segment in an EDI document.
    /// </summary>
    public sealed class EdiSegment
    {
        public string Tag { get; }
        public string RawText { get; }
        public int StartOffset { get; }
        public int EndOffset { get; }
        public IReadOnlyList<EdiElement> Elements { get; }
        public string? Label { get; }

        public EdiSegment(string tag, string rawText, int startOffset, int endOffset, IReadOnlyList<EdiElement>? elements = null, string? label = null)
        {
            Tag = tag;
            RawText = rawText;
            StartOffset = startOffset;
            EndOffset = endOffset;
            Elements = elements ?? new List<EdiElement>();
            Label = label;
        }
    }
}
