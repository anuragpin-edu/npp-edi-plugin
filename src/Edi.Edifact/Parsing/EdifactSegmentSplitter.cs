using System;
using System.Collections.Generic;
using Edi.Core.Model;

namespace Edi.Edifact.Parsing
{
    /// <summary>
    /// Splits raw EDIFACT text into individual segment spans, respecting the release character.
    /// </summary>
    public static class EdifactSegmentSplitter
    {
        /// <summary>
        /// Splits the EDIFACT text into raw segments starting from <paramref name="startOffset"/>.
        /// </summary>
        /// <param name="text">The complete raw EDI text.</param>
        /// <param name="delimiters">The delimiters in effect for this interchange.</param>
        /// <param name="startOffset">
        /// The character index at which to begin scanning (typically past the UNA segment).
        /// </param>
        /// <returns>
        /// A list of tuples containing the raw segment text (trimmed),
        /// the start offset of the first non-whitespace character, and
        /// the offset of the segment terminator in the original text.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="delimiters"/> is null.
        /// </exception>
        public static List<(string RawText, int StartOffset, int EndOffset)> Split(
            string text, EdiDelimiters delimiters, int startOffset)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (delimiters == null)
                throw new ArgumentNullException(nameof(delimiters));

            var results = new List<(string RawText, int StartOffset, int EndOffset)>();
            char terminator = delimiters.SegmentTerminator;
            char release = delimiters.ReleaseCharacter;

            int segmentContentStart = startOffset;
            bool released = false;

            for (int i = startOffset; i < text.Length; i++)
            {
                char c = text[i];

                if (released)
                {
                    // The current character is escaped — treat it as literal content.
                    released = false;
                    continue;
                }

                if (c == release)
                {
                    released = true;
                    continue;
                }

                if (c == terminator)
                {
                    // Extract the segment text between segmentContentStart and i (exclusive).
                    string raw = text.Substring(segmentContentStart, i - segmentContentStart);
                    string trimmed = raw.Trim();

                    if (trimmed.Length > 0)
                    {
                        // Compute the offset of the first non-whitespace character.
                        int firstNonWs = segmentContentStart;
                        while (firstNonWs < i && char.IsWhiteSpace(text[firstNonWs]))
                            firstNonWs++;

                        results.Add((trimmed, firstNonWs, i));
                    }

                    segmentContentStart = i + 1;
                }
            }

            // Handle trailing content with no final terminator.
            if (segmentContentStart < text.Length)
            {
                string raw = text.Substring(segmentContentStart);
                string trimmed = raw.Trim();

                if (trimmed.Length > 0)
                {
                    int firstNonWs = segmentContentStart;
                    while (firstNonWs < text.Length && char.IsWhiteSpace(text[firstNonWs]))
                        firstNonWs++;

                    // EndOffset is the last character index in the segment text.
                    results.Add((trimmed, firstNonWs, text.Length - 1));
                }
            }

            return results;
        }
    }
}
