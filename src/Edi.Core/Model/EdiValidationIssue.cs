namespace Edi.Core.Model
{
    /// <summary>
    /// Represents a validation issue discovered during EDI parsing.
    /// </summary>
    public sealed class EdiValidationIssue
    {
        public string Code { get; }
        public string Message { get; }
        public int StartOffset { get; }
        public int EndOffset { get; }
        public EdiIssueSeverity Severity { get; }

        public EdiValidationIssue(string code, string message, int startOffset, int endOffset, EdiIssueSeverity severity)
        {
            Code = code;
            Message = message;
            StartOffset = startOffset;
            EndOffset = endOffset;
            Severity = severity;
        }
    }
}
