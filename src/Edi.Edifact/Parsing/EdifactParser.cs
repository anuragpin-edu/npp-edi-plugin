using System;
using System.Collections.Generic;
using Edi.Core.Dictionary;
using Edi.Core.Model;
using Edi.Core.Parsing;
using Edi.Edifact.Detection;
using Edi.Edifact.Validation;

namespace Edi.Edifact.Parsing
{
    /// <summary>
    /// Parses raw EDIFACT text into an <see cref="EdiDocument"/>, including segment splitting,
    /// element/component parsing, label resolution, and envelope validation.
    /// </summary>
    public sealed class EdifactParser : IEdiParser
    {
        /// <summary>
        /// Determines whether this parser can handle the given text by checking for
        /// EDIFACT indicators (UNA or UNB).
        /// </summary>
        /// <param name="text">The raw text to inspect.</param>
        /// <returns><c>true</c> if the text appears to be EDIFACT; otherwise <c>false</c>.</returns>
        public bool CanParse(string text)
        {
            return EdifactDelimiterDetector.IsEdifact(text);
        }

        /// <summary>
        /// Parses raw EDIFACT text into an <see cref="EdiDocument"/>.
        /// </summary>
        /// <param name="text">The raw EDIFACT interchange text.</param>
        /// <param name="dictionary">
        /// Optional dictionary for resolving segment, element, and component labels.
        /// </param>
        /// <returns>A fully parsed <see cref="EdiDocument"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> is null.
        /// </exception>
        public EdiDocument Parse(string text, IEdiDictionary? dictionary)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            // Step 1: Detect delimiters (from UNA or defaults).
            EdiDelimiters delimiters = EdifactDelimiterDetector.Detect(text);

            // Step 2: Determine where data begins (past UNA if present).
            int dataStart = EdifactDelimiterDetector.GetDataStartOffset(text);

            // Step 3: Split text into raw segments.
            var rawSegments = EdifactSegmentSplitter.Split(text, delimiters, dataStart);

            // Step 4: Parse each raw segment into an EdiSegment.
            var segments = new List<EdiSegment>(rawSegments.Count);

            foreach (var (rawText, startOffset, endOffset) in rawSegments)
            {
                string tag;
                string? body;
                ExtractTagAndBody(rawText, delimiters.ElementSeparator, out tag, out body);

                // Parse elements from the body.
                List<EdiElement> elements = body != null
                    ? EdifactElementParser.ParseElements(body, delimiters, dictionary, EdiStandard.Edifact, tag)
                    : new List<EdiElement>();

                // Look up segment label from dictionary.
                string? segmentLabel = dictionary?.GetSegmentLabel(EdiStandard.Edifact, tag);

                segments.Add(new EdiSegment(
                    tag,
                    rawText,
                    startOffset,
                    endOffset,
                    elements.Count > 0 ? (IReadOnlyList<EdiElement>)elements : null,
                    segmentLabel));
            }

            // Step 5: Run envelope validation.
            var validator = new EdifactEnvelopeValidator();
            IReadOnlyList<EdiValidationIssue> issues;

            // Create a temporary document for validation (without issues).
            var tempDoc = new EdiDocument(
                EdiStandard.Edifact,
                segments,
                null,
                delimiters);

            issues = validator.Validate(tempDoc);

            // Step 6: Build and return the final document.
            return new EdiDocument(
                EdiStandard.Edifact,
                segments,
                issues.Count > 0 ? issues : null,
                delimiters);
        }

        /// <summary>
        /// Extracts the segment tag and body from the raw segment text.
        /// The tag is everything before the first element separator;
        /// the body is everything after it.
        /// </summary>
        /// <param name="rawText">The raw segment text (e.g., "BGM+220+PO12345+9").</param>
        /// <param name="elementSeparator">The element separator character.</param>
        /// <param name="tag">Output: the segment tag (e.g., "BGM").</param>
        /// <param name="body">
        /// Output: the segment body after the first separator (e.g., "220+PO12345+9"),
        /// or null if there is no separator.
        /// </param>
        private static void ExtractTagAndBody(
            string rawText, char elementSeparator, out string tag, out string? body)
        {
            int sepIndex = rawText.IndexOf(elementSeparator);
            if (sepIndex < 0)
            {
                tag = rawText;
                body = null;
            }
            else
            {
                tag = rawText.Substring(0, sepIndex);
                body = rawText.Substring(sepIndex + 1);
            }
        }
    }
}
