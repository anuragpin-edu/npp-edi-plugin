using Edi.Core.Model;

namespace Edi.Core.Dictionary
{
    /// <summary>
    /// Defines methods for resolving labels from an EDI dictionary.
    /// </summary>
    public interface IEdiDictionary
    {
        string? GetSegmentLabel(EdiStandard standard, string segmentTag);
        string? GetElementLabel(EdiStandard standard, string segmentTag, int position);
        string? GetComponentLabel(EdiStandard standard, string segmentTag, int elementPosition, int componentIndex);
        string? GetQualifierLabel(EdiStandard standard, string segmentTag, int elementPosition, string value);
    }
}
