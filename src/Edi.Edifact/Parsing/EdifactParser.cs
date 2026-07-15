using System;
using System.Collections.Generic;
using Edi.Core.Dictionary;
using Edi.Core.Model;
using Edi.Core.Parsing;
using Edi.Edifact.Detection;
using Edi.Edifact.Validation;

namespace Edi.Edifact.Parsing
{
    public sealed class EdifactParser : IEdiParser
    {
        public bool CanParse(string text)
        {
            return EdifactDelimiterDetector.IsEdifact(text);
        }

        public EdiDocument Parse(string text, IEdiDictionary? dictionary)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            EdiDelimiters delimiters = EdifactDelimiterDetector.Detect(text);
            int dataStart = EdifactDelimiterDetector.GetDataStartOffset(text);
            var rawSegments = EdifactSegmentSplitter.Split(text, delimiters, dataStart);

            // Extract version from UNH segment
            string? version = null;
            foreach (var (rawText, _, _) in rawSegments)
            {
                ExtractTagAndBody(rawText, delimiters.ElementSeparator, out string tag, out string? body);
                if (tag == "UNH" && body != null)
                {
                    var elements = body.Split(delimiters.ElementSeparator);
                    if (elements.Length >= 2)
                    {
                        var comps = elements[1].Split(delimiters.ComponentSeparator);
                        if (comps.Length >= 3)
                        {
                            version = comps[1] + comps[2]; // e.g. D96A
                        }
                    }
                    break;
                }
            }

            var segments = new List<EdiSegment>(rawSegments.Count);

            foreach (var (rawText, startOffset, endOffset) in rawSegments)
            {
                ExtractTagAndBody(rawText, delimiters.ElementSeparator, out string tag, out string? body);

                List<EdiElement> elements = body != null
                    ? EdifactElementParser.ParseElements(body, delimiters, dictionary, version, EdiStandard.Edifact, tag)
                    : new List<EdiElement>();

                string? segmentLabel = dictionary?.GetSegmentLabel(EdiStandard.Edifact, version, tag);

                segments.Add(new EdiSegment(
                    tag,
                    rawText,
                    startOffset,
                    endOffset,
                    elements.Count > 0 ? (IReadOnlyList<EdiElement>)elements : null,
                    segmentLabel));
            }

            var validator = new EdifactEnvelopeValidator();
            var tempDoc = new EdiDocument(EdiStandard.Edifact, version, segments, null, delimiters);
            var issues = validator.Validate(tempDoc);

            return new EdiDocument(EdiStandard.Edifact, version, segments, issues.Count > 0 ? issues : null, delimiters);
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
