using System.Collections.Generic;
using Edi.Core.Model;

namespace Edi.X12.Parsing
{
    public static class X12SegmentSplitter
    {
        public static IReadOnlyList<(string RawText, int StartOffset, int EndOffset)> Split(string text, EdiDelimiters delimiters, int dataStartOffset)
        {
            var segments = new List<(string RawText, int StartOffset, int EndOffset)>();
            if (string.IsNullOrEmpty(text)) return segments;

            char terminator = delimiters.SegmentTerminator;
            int start = dataStartOffset;

            for (int i = dataStartOffset; i < text.Length; i++)
            {
                if (text[i] == terminator)
                {
                    int end = i;
                    string rawText = text.Substring(start, end - start).TrimStart('\r', '\n', ' ', '\t');
                    string rawOriginalText = text.Substring(start, end - start);
                    
                    int padding = 0;
                    for (int j = 0; j < rawOriginalText.Length; j++)
                    {
                        if (rawOriginalText[j] == '\r' || rawOriginalText[j] == '\n' || rawOriginalText[j] == ' ' || rawOriginalText[j] == '\t')
                            padding++;
                        else
                            break;
                    }

                    if (!string.IsNullOrWhiteSpace(rawText))
                    {
                        segments.Add((rawText.TrimEnd(), start + padding, end));
                    }
                    start = i + 1;
                }
            }

            if (start < text.Length)
            {
                string rawText = text.Substring(start).TrimStart('\r', '\n', ' ', '\t');
                if (!string.IsNullOrWhiteSpace(rawText))
                {
                    // If it doesn't end with a terminator, we still might want to capture it, but normally it should.
                    int padding = 0;
                    string rawOriginalText = text.Substring(start);
                    for (int j = 0; j < rawOriginalText.Length; j++)
                    {
                        if (rawOriginalText[j] == '\r' || rawOriginalText[j] == '\n' || rawOriginalText[j] == ' ' || rawOriginalText[j] == '\t')
                            padding++;
                        else
                            break;
                    }

                    // Trim end as well to ignore trailing spaces.
                    segments.Add((rawText.TrimEnd(), start + padding, text.Length));
                }
            }

            return segments;
        }
    }
}
