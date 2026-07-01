using System.Collections.Generic;

namespace Edi.Core.Model
{
    /// <summary>
    /// Represents a data element in an EDI segment.
    /// </summary>
    public sealed class EdiElement
    {
        public int Position { get; }
        public string RawValue { get; }
        public string? Label { get; }
        public IReadOnlyList<EdiComponent> Components { get; }

        public EdiElement(int position, string rawValue, string? label = null, IReadOnlyList<EdiComponent>? components = null)
        {
            Position = position;
            RawValue = rawValue;
            Label = label;
            Components = components ?? new List<EdiComponent>();
        }
    }
}
