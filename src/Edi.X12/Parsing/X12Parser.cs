using System;
using System.Collections.Generic;
using Edi.Core.Dictionary;
using Edi.Core.Model;
using Edi.Core.Parsing;
using Edi.X12.Detection;
using Edi.X12.Validation;

namespace Edi.X12.Parsing
{
    public sealed class X12Parser : IEdiParser
    {
        public bool CanParse(string text)
        {
            return X12DelimiterDetector.IsX12(text);
        }

        public EdiDocument Parse(string text, IEdiDictionary? dictionary)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            EdiDelimiters delimiters = X12DelimiterDetector.Detect(text);
            var rawSegments = X12SegmentSplitter.Split(text, delimiters.SegmentTerminator);

            // Extract version from GS or ISA
            string? version = null;
            foreach (var (rawText, _, _) in rawSegments)
            {
                ExtractTagAndBody(rawText, delimiters.ElementSeparator, out string tag, out string? body);
                if (tag == "GS" && body != null)
                {
                    var elements = body.Split(delimiters.ElementSeparator);
                    if (elements.Length >= 8)
                    {
                        version = elements[7];
                        if (version.EndsWith("0")) version = version.Substring(0, version.Length - 1); // 004010 -> 00401
                    }
                    break;
                }
                else if (tag == "ISA" && body != null)
                {
                    var elements = body.Split(delimiters.ElementSeparator);
                    if (elements.Length >= 12 && version == null)
                    {
                        version = elements[11];
                    }
                }
            }

            var segments = new List<EdiSegment>(rawSegments.Count);

            foreach (var (rawText, startOffset, endOffset) in rawSegments)
            {
                ExtractTagAndBody(rawText, delimiters.ElementSeparator, out string tag, out string? body);

                List<EdiElement> elements = body != null
                    ? X12ElementParser.ParseElements(body, delimiters.ElementSeparator, delimiters.ComponentSeparator, dictionary, version, tag)
                    : new List<EdiElement>();

                string? segmentLabel = dictionary?.GetSegmentLabel(EdiStandard.X12, version, tag);

                segments.Add(new EdiSegment(
                    tag,
                    rawText,
                    startOffset,
                    endOffset,
                    elements.Count > 0 ? (IReadOnlyList<EdiElement>)elements : null,
                    segmentLabel));
            }

            var validator = new X12EnvelopeValidator();
            var tempDoc = new EdiDocument(EdiStandard.X12, version, segments, null, delimiters);
            var issues = validator.Validate(tempDoc);

            return new EdiDocument(EdiStandard.X12, version, segments, issues.Count > 0 ? issues : null, delimiters);
        }

        private static void ExtractTagAndBody(string rawText, char elementSeparator, out string tag, out string? body)
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
