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

        public EdiComponent(int index, string rawValue, string? label = null)
        {
            Index = index;
            RawValue = rawValue;
            Label = label;
        }
    }
}
