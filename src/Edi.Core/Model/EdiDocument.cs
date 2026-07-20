using System.Collections.Generic;

namespace Edi.Core.Model
{
    /// <summary>
    /// Represents a parsed EDI document.
    /// </summary>
    public sealed class EdiDocument
    {
        public EdiStandard Standard { get; }
        public string? Version { get; }
        public string? DictionaryRelease { get; }
        public IReadOnlyList<EdiSegment> Segments { get; }
        public IReadOnlyList<EdiValidationIssue> Issues { get; }
        public EdiDelimiters? Delimiters { get; }

        public EdiDocument(EdiStandard standard, string? version, string? dictionaryRelease, IReadOnlyList<EdiSegment> segments, IReadOnlyList<EdiValidationIssue>? issues = null, EdiDelimiters? delimiters = null)
        {
            Standard = standard;
            Version = version;
            DictionaryRelease = dictionaryRelease;
            Segments = segments;
            Issues = issues ?? new List<EdiValidationIssue>();
            Delimiters = delimiters;
        }
    }
}
