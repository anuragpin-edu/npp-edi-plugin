using Edi.Core.Model;

namespace Edi.Core.Dictionary
{
    /// <summary>
    /// An empty dictionary that returns null for all lookups.
    /// </summary>
    public class NullEdiDictionary : IEdiDictionary
    {
        public string? GetSegmentLabel(EdiStandard standard, string segmentTag) => null;
        public string? GetElementLabel(EdiStandard standard, string segmentTag, int position) => null;
        public string? GetComponentLabel(EdiStandard standard, string segmentTag, int elementPosition, int componentIndex) => null;
        public string? GetQualifierLabel(EdiStandard standard, string segmentTag, int elementPosition, string value) => null;
    }
}
