using System;
using Edi.Core.Model;

namespace Edi.Edifact.Detection
{
    /// <summary>
    /// Detects EDIFACT delimiters from the UNA service string and identifies EDIFACT content.
    /// </summary>
    public static class EdifactDelimiterDetector
    {
        /// <summary>
        /// The length of the UNA service string advice segment (UNA + 6 delimiter characters).
        /// </summary>
        private const int UnaLength = 9;

        /// <summary>
        /// Detects delimiters from the UNA service string advice.
        /// If no UNA segment is present, returns default EDIFACT delimiters.
        /// </summary>
        /// <param name="text">The raw EDI text to inspect.</param>
        /// <returns>The detected <see cref="EdiDelimiters"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
        public static EdiDelimiters Detect(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (text.Length >= UnaLength && text.StartsWith("UNA", StringComparison.Ordinal))
            {
                // UNA positions:
                //   [0]='U' [1]='N' [2]='A'
                //   [3] = component separator     (default ':')
                //   [4] = element separator        (default '+')
                //   [5] = decimal mark             (default '.')
                //   [6] = release character        (default '?')
                //   [7] = reserved                 (ignored)
                //   [8] = segment terminator       (default '\'')
                char componentSep = text[3];
                char elementSep = text[4];
                char decimalMark = text[5];
                char releaseChar = text[6];
                // text[7] is reserved — skipped
                char segmentTerminator = text[8];

                return new EdiDelimiters(componentSep, elementSep, decimalMark, releaseChar, segmentTerminator);
            }

            return EdiDelimiters.EdifactDefaults;
        }

        /// <summary>
        /// Determines whether the given text is EDIFACT content by checking if it starts
        /// with a UNA service string advice or a UNB interchange header.
        /// </summary>
        /// <param name="text">The raw text to inspect.</param>
        /// <returns><c>true</c> if the text appears to be EDIFACT; otherwise <c>false</c>.</returns>
        public static bool IsEdifact(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            string trimmed = text.TrimStart();
            return trimmed.StartsWith("UNA", StringComparison.Ordinal)
                || trimmed.StartsWith("UNB", StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns the byte offset where the data payload begins, past any UNA segment.
        /// </summary>
        /// <param name="text">The raw EDI text to inspect.</param>
        /// <returns>9 if the text starts with a UNA service string; otherwise 0.</returns>
        public static int GetDataStartOffset(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            return text.StartsWith("UNA", StringComparison.Ordinal) ? UnaLength : 0;
        }
    }
}
