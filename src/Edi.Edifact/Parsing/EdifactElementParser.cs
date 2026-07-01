using System;
using System.Collections.Generic;
using Edi.Core.Dictionary;
using Edi.Core.Model;

namespace Edi.Edifact.Parsing
{
    /// <summary>
    /// Parses the body of an EDIFACT segment into elements and components,
    /// respecting release-character escaping and resolving labels via the dictionary.
    /// </summary>
    public static class EdifactElementParser
    {
        /// <summary>
        /// Parses the segment body (everything after tag + first element separator) into elements.
        /// </summary>
        /// <param name="segmentBody">
        /// The segment content after the tag and first element separator.
        /// For example, for segment "BGM+220+PO12345+9", this is "220+PO12345+9".
        /// </param>
        /// <param name="delimiters">The delimiters in effect.</param>
        /// <param name="dictionary">Optional dictionary for resolving labels.</param>
        /// <param name="standard">The EDI standard (should be <see cref="EdiStandard.Edifact"/>).</param>
        /// <param name="segmentTag">The segment tag (e.g., "BGM") for dictionary lookups.</param>
        /// <returns>A list of parsed <see cref="EdiElement"/> instances with 1-based positions.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="delimiters"/> is null.
        /// </exception>
        public static List<EdiElement> ParseElements(
            string segmentBody,
            EdiDelimiters delimiters,
            IEdiDictionary? dictionary,
            EdiStandard standard,
            string segmentTag)
        {
            if (delimiters == null)
                throw new ArgumentNullException(nameof(delimiters));

            var elements = new List<EdiElement>();

            if (string.IsNullOrEmpty(segmentBody))
                return elements;

            var elementStrings = SplitRespectingRelease(
                segmentBody, delimiters.ElementSeparator, delimiters.ReleaseCharacter);

            for (int i = 0; i < elementStrings.Count; i++)
            {
                int position = i + 1; // 1-based
                string rawElement = elementStrings[i];

                // Split element into components.
                var componentStrings = SplitRespectingRelease(
                    rawElement, delimiters.ComponentSeparator, delimiters.ReleaseCharacter);

                List<EdiComponent>? components = null;
                if (componentStrings.Count > 1)
                {
                    // Only create component list when there are multiple components.
                    components = new List<EdiComponent>(componentStrings.Count);
                    for (int j = 0; j < componentStrings.Count; j++)
                    {
                        string? componentLabel = dictionary?.GetComponentLabel(
                            standard, segmentTag, position, j);
                        components.Add(new EdiComponent(j, componentStrings[j], componentLabel));
                    }
                }

                string? elementLabel = dictionary?.GetElementLabel(standard, segmentTag, position);

                elements.Add(new EdiElement(
                    position,
                    rawElement,
                    elementLabel,
                    components != null
                        ? (IReadOnlyList<EdiComponent>)components
                        : null));
            }

            return elements;
        }

        /// <summary>
        /// Splits a string on the specified separator character, respecting the release character.
        /// A release character preceding the separator means the separator is literal content.
        /// A release character preceding another release character is a literal release character.
        /// </summary>
        /// <param name="input">The string to split.</param>
        /// <param name="separator">The separator character.</param>
        /// <param name="release">The release (escape) character.</param>
        /// <returns>A list of the split parts, preserving the original text (including release characters).</returns>
        internal static List<string> SplitRespectingRelease(string input, char separator, char release)
        {
            var parts = new List<string>();

            if (string.IsNullOrEmpty(input))
            {
                parts.Add(string.Empty);
                return parts;
            }

            int partStart = 0;
            bool released = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

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

                if (c == separator)
                {
                    parts.Add(input.Substring(partStart, i - partStart));
                    partStart = i + 1;
                }
            }

            // Add the final part.
            parts.Add(input.Substring(partStart));
            return parts;
        }
    }
}
