namespace Edi.Core.Model
{
    /// <summary>
    /// Represents a sub-element component in an EDI document.
    /// </summary>
    public sealed class EdiComponent
    {
        public int Index { get; }
        public string RawValue { get; }
        public string? Label { get; }
        public string? ValueDescription { get; }

        public EdiComponent(int index, string rawValue, string? label = null, string? valueDescription = null)
        {
            Index = index;
            RawValue = rawValue;
            Label = label;
            ValueDescription = valueDescription;
        }
    }
}
