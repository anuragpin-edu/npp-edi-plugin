using System;
using Edi.Core.Model;

namespace Edi.X12.Detection
{
    /// <summary>
    /// Detects X12 delimiters from the ISA segment and identifies X12 content.
    /// </summary>
    public static class X12DelimiterDetector
    {
        private const int IsaLength = 106;

        private static readonly int[] ElementSeparatorPositions = new int[]
        {
            6, 17, 20, 31, 34, 50, 53, 69, 76, 81, 83, 89, 99, 101, 103
        };

        /// <summary>
        /// Determines whether the given text is X12 content by checking if it starts
        /// with an ISA segment, meets length requirements, and has a valid ISA structure.
        /// </summary>
        public static bool IsX12(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            string trimmed = text.TrimStart();
            if (trimmed.Length < IsaLength || !trimmed.StartsWith("ISA", StringComparison.Ordinal))
            {
                return false;
            }

            char elementSep = trimmed[3];
            foreach (int pos in ElementSeparatorPositions)
            {
                if (trimmed[pos] != elementSep)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Detects delimiters from the ISA segment.
        /// </summary>
        public static EdiDelimiters Detect(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            string trimmed = text.TrimStart();
            if (trimmed.Length >= IsaLength && trimmed.StartsWith("ISA", StringComparison.Ordinal))
            {
                char elementSep = trimmed[3];
                char componentSep = trimmed[104];
                char segmentTerminator = trimmed[105];

                return new EdiDelimiters(componentSep, elementSep, '\0', '\0', segmentTerminator);
            }

            // Defaults if not enough data
            return new EdiDelimiters('>', '*', '\0', '\0', '~');
        }

        public static int GetDataStartOffset(string text)
        {
            return 0; // The ISA segment is part of the data.
        }
    }
}
