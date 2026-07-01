namespace Edi.Core.Model
{
    /// <summary>
    /// Represents the structural delimiters for an EDI document.
    /// </summary>
    public sealed class EdiDelimiters
    {
        public char ComponentSeparator { get; }
        public char ElementSeparator { get; }
        public char DecimalMark { get; }
        public char ReleaseCharacter { get; }
        public char SegmentTerminator { get; }

        public EdiDelimiters(char componentSeparator, char elementSeparator, char decimalMark, char releaseCharacter, char segmentTerminator)
        {
            ComponentSeparator = componentSeparator;
            ElementSeparator = elementSeparator;
            DecimalMark = decimalMark;
            ReleaseCharacter = releaseCharacter;
            SegmentTerminator = segmentTerminator;
        }

        public static EdiDelimiters EdifactDefaults { get; } = new EdiDelimiters(':', '+', '.', '?', '\'');
    }
}
