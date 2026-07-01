using System;
using System.Text;
using Edi.Core.Formatting;
using Edi.Core.Model;
using Edi.Edifact.Detection;

namespace Edi.Edifact.Formatting
{
    /// <summary>
    /// Formats EDIFACT interchanges: prettifies into one-segment-per-line layout
    /// or minifies into a compact single-line format.
    /// </summary>
    public sealed class EdifactFormatter : IEdiFormatter
    {
        /// <summary>
        /// Formats the EDIFACT text so each segment appears on its own line.
        /// The UNA service string (if present) appears as the first line.
        /// </summary>
        /// <param name="text">The raw EDIFACT text.</param>
        /// <returns>The prettified EDIFACT text with one segment per line.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
        public string Prettify(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (string.IsNullOrWhiteSpace(text))
                return text;

            EdiDelimiters delimiters = EdifactDelimiterDetector.Detect(text);
            int dataStart = EdifactDelimiterDetector.GetDataStartOffset(text);

            var sb = new StringBuilder(text.Length + 64);

            // Output UNA line if present.
            if (dataStart > 0)
            {
                sb.Append(text, 0, dataStart);
                sb.AppendLine();
            }

            // Split the data portion into segments respecting the release character.
            string dataPart = text.Substring(dataStart);
            var segmentTexts = SplitOnTerminator(dataPart, delimiters);

            for (int i = 0; i < segmentTexts.Count; i++)
            {
                string trimmed = segmentTexts[i].Trim();
                if (trimmed.Length == 0)
                    continue;

                sb.Append(trimmed);
                sb.Append(delimiters.SegmentTerminator);
                sb.AppendLine();
            }

            // Trim trailing empty lines.
            while (sb.Length > 0)
            {
                char last = sb[sb.Length - 1];
                if (last == '\n' || last == '\r' || last == ' ')
                    sb.Length--;
                else
                    break;
            }

            // Ensure exactly one trailing newline.
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Minifies the EDIFACT text into a compact format with no whitespace between segments.
        /// </summary>
        /// <param name="text">The raw EDIFACT text.</param>
        /// <returns>The minified EDIFACT text.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
        public string Minify(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (string.IsNullOrWhiteSpace(text))
                return text;

            EdiDelimiters delimiters = EdifactDelimiterDetector.Detect(text);
            int dataStart = EdifactDelimiterDetector.GetDataStartOffset(text);

            var sb = new StringBuilder(text.Length);

            // Output UNA if present (exactly 9 characters, no whitespace).
            if (dataStart > 0)
            {
                sb.Append(text, 0, dataStart);
            }

            // Split the data portion into segments respecting the release character.
            string dataPart = text.Substring(dataStart);
            var segmentTexts = SplitOnTerminator(dataPart, delimiters);

            foreach (string seg in segmentTexts)
            {
                string trimmed = seg.Trim();
                if (trimmed.Length == 0)
                    continue;

                sb.Append(trimmed);
                sb.Append(delimiters.SegmentTerminator);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Splits text on the segment terminator while respecting the release character.
        /// Returns the text portions between terminators (without the terminator itself).
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <param name="delimiters">The delimiters in effect.</param>
        /// <returns>A list of segment text portions.</returns>
        private static System.Collections.Generic.List<string> SplitOnTerminator(
            string text, EdiDelimiters delimiters)
        {
            var parts = new System.Collections.Generic.List<string>();
            char terminator = delimiters.SegmentTerminator;
            char release = delimiters.ReleaseCharacter;

            int start = 0;
            bool released = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (released)
                {
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
                    parts.Add(text.Substring(start, i - start));
                    start = i + 1;
                }
            }

            // Remaining text after last terminator.
            if (start < text.Length)
            {
                parts.Add(text.Substring(start));
            }

            return parts;
        }
    }
}
