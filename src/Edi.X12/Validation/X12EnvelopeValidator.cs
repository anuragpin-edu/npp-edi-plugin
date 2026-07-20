using System;
using System.Collections.Generic;
using Edi.Core.Model;
using Edi.Core.Validation;

namespace Edi.X12.Validation
{
    public sealed class X12EnvelopeValidator : IEdiValidator
    {
        public IReadOnlyList<EdiValidationIssue> Validate(EdiDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var issues = new List<EdiValidationIssue>();
            var segments = document.Segments;
            if (segments == null || segments.Count == 0) return issues;

            EdiSegment? isaSegment = null;
            if (!string.Equals(segments[0].Tag, "ISA", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new EdiValidationIssue("X12_MISSING_ISA", "Interchange must begin with an ISA segment.", segments[0].StartOffset, segments[0].EndOffset, EdiIssueSeverity.Error));
            }
            else
            {
                isaSegment = segments[0];
            }

            EdiSegment? ieaSegment = null;
            var lastSeg = segments[segments.Count - 1];
            if (!string.Equals(lastSeg.Tag, "IEA", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new EdiValidationIssue("X12_MISSING_IEA", "Interchange must end with an IEA segment.", lastSeg.StartOffset, lastSeg.EndOffset, EdiIssueSeverity.Error));
            }
            else
            {
                ieaSegment = lastSeg;
            }

            if (isaSegment != null && ieaSegment != null)
            {
                string? isaRef = GetElementValue(isaSegment, 13);
                string? ieaRef = GetElementValue(ieaSegment, 2);
                if (isaRef != null && ieaRef != null && !string.Equals(isaRef, ieaRef, StringComparison.Ordinal))
                {
                    issues.Add(new EdiValidationIssue("X12_ISA_IEA_MISMATCH", $"IEA reference '{ieaRef}' does not match ISA reference '{isaRef}'.", ieaSegment.StartOffset, ieaSegment.EndOffset, EdiIssueSeverity.Error));
                }
            }

            int gsCount = 0;
            int i = 0;
            while (i < segments.Count)
            {
                if (string.Equals(segments[i].Tag, "GS", StringComparison.OrdinalIgnoreCase))
                {
                    gsCount++;
                    var gsSegment = segments[i];
                    int gsIndex = i;
                    int geIndex = -1;

                    for (int j = i + 1; j < segments.Count; j++)
                    {
                        if (string.Equals(segments[j].Tag, "GS", StringComparison.OrdinalIgnoreCase)) break;
                        if (string.Equals(segments[j].Tag, "GE", StringComparison.OrdinalIgnoreCase))
                        {
                            geIndex = j;
                            break;
                        }
                    }

                    if (geIndex < 0)
                    {
                        issues.Add(new EdiValidationIssue("X12_MISSING_GE", $"GS at offset {gsSegment.StartOffset} has no matching GE segment.", gsSegment.StartOffset, gsSegment.EndOffset, EdiIssueSeverity.Error));
                    }
                    else
                    {
                        var geSegment = segments[geIndex];
                        string? gsRef = GetElementValue(gsSegment, 6);
                        string? geRef = GetElementValue(geSegment, 2);
                        if (gsRef != null && geRef != null && !string.Equals(gsRef, geRef, StringComparison.Ordinal))
                        {
                            issues.Add(new EdiValidationIssue("X12_GS_GE_MISMATCH", $"GE reference '{geRef}' does not match GS reference '{gsRef}'.", geSegment.StartOffset, geSegment.EndOffset, EdiIssueSeverity.Error));
                        }

                        // Check ST/SE inside GS/GE
                        int stCount = 0;
                        int k = gsIndex + 1;
                        while (k < geIndex)
                        {
                            if (string.Equals(segments[k].Tag, "ST", StringComparison.OrdinalIgnoreCase))
                            {
                                stCount++;
                                var stSegment = segments[k];
                                int stIndex = k;
                                int seIndex = -1;
                                for (int m = k + 1; m < geIndex; m++)
                                {
                                    if (string.Equals(segments[m].Tag, "ST", StringComparison.OrdinalIgnoreCase)) break;
                                    if (string.Equals(segments[m].Tag, "SE", StringComparison.OrdinalIgnoreCase))
                                    {
                                        seIndex = m;
                                        break;
                                    }
                                }

                                if (seIndex < 0)
                                {
                                    issues.Add(new EdiValidationIssue("X12_MISSING_SE", $"ST at offset {stSegment.StartOffset} has no matching SE segment.", stSegment.StartOffset, stSegment.EndOffset, EdiIssueSeverity.Error));
                                }
                                else
                                {
                                    var seSegment = segments[seIndex];
                                    string? stRef = GetElementValue(stSegment, 2);
                                    string? seRef = GetElementValue(seSegment, 2);
                                    if (stRef != null && seRef != null && !string.Equals(stRef, seRef, StringComparison.Ordinal))
                                    {
                                        issues.Add(new EdiValidationIssue("X12_ST_SE_MISMATCH", $"SE reference '{seRef}' does not match ST reference '{stRef}'.", seSegment.StartOffset, seSegment.EndOffset, EdiIssueSeverity.Error));
                                    }

                                    int expectedSeCount = seIndex - stIndex + 1;
                                    string? seCountStr = GetElementValue(seSegment, 1);
                                    if (seCountStr != null && int.TryParse(seCountStr, out int actualSeCount) && actualSeCount != expectedSeCount)
                                    {
                                        issues.Add(new EdiValidationIssue("X12_SE_COUNT_MISMATCH", $"SE count mismatch: declared {actualSeCount}, actual {expectedSeCount}.", seSegment.StartOffset, seSegment.EndOffset, EdiIssueSeverity.Error));
                                    }

                                    k = seIndex;
                                }
                            }
                            k++;
                        }

                        string? geCountStr = GetElementValue(geSegment, 1);
                        if (geCountStr != null && int.TryParse(geCountStr, out int actualGeCount) && actualGeCount != stCount)
                        {
                            issues.Add(new EdiValidationIssue("X12_GE_COUNT_MISMATCH", $"GE count mismatch: declared {actualGeCount}, actual {stCount}.", geSegment.StartOffset, geSegment.EndOffset, EdiIssueSeverity.Error));
                        }
                    }
                }
                i++;
            }

            if (ieaSegment != null)
            {
                string? ieaCountStr = GetElementValue(ieaSegment, 1);
                if (ieaCountStr != null && int.TryParse(ieaCountStr, out int actualIeaCount) && actualIeaCount != gsCount)
                {
                    issues.Add(new EdiValidationIssue("X12_IEA_COUNT_MISMATCH", $"IEA count mismatch: declared {actualIeaCount}, actual {gsCount}.", ieaSegment.StartOffset, ieaSegment.EndOffset, EdiIssueSeverity.Error));
                }
            }

            return issues;
        }

        private static string? GetElementValue(EdiSegment segment, int position)
        {
            if (segment.Elements == null) return null;
            foreach (var element in segment.Elements)
            {
                if (element.Position == position)
                {
                    if (element.Components != null && element.Components.Count > 0)
                        return element.Components[0].RawValue;
                    return element.RawValue;
                }
            }
            return null;
        }
    }
}
