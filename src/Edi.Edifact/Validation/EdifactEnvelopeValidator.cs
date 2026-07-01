using System;
using System.Collections.Generic;
using Edi.Core.Model;
using Edi.Core.Validation;

namespace Edi.Edifact.Validation
{
    /// <summary>
    /// Validates the EDIFACT envelope structure: UNB/UNZ interchange wrapper
    /// and UNH/UNT message pairs, including segment counts and reference matching.
    /// </summary>
    public sealed class EdifactEnvelopeValidator : IEdiValidator
    {
        /// <summary>
        /// Validates the envelope structure of the given EDIFACT document.
        /// </summary>
        /// <param name="document">The parsed EDIFACT document to validate.</param>
        /// <returns>A list of validation issues found in the envelope structure.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="document"/> is null.
        /// </exception>
        public IReadOnlyList<EdiValidationIssue> Validate(EdiDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var issues = new List<EdiValidationIssue>();
            var segments = document.Segments;

            if (segments == null || segments.Count == 0)
                return issues;

            // --- UNB check ---
            EdiSegment? unbSegment = null;
            if (!string.Equals(segments[0].Tag, "UNB", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new EdiValidationIssue(
                    "EDIFACT_MISSING_UNB",
                    "Interchange must begin with a UNB segment.",
                    segments[0].StartOffset,
                    segments[0].EndOffset,
                    EdiIssueSeverity.Error));
            }
            else
            {
                unbSegment = segments[0];
            }

            // --- UNZ check ---
            EdiSegment? unzSegment = null;
            if (!string.Equals(segments[segments.Count - 1].Tag, "UNZ", StringComparison.OrdinalIgnoreCase))
            {
                var lastSeg = segments[segments.Count - 1];
                issues.Add(new EdiValidationIssue(
                    "EDIFACT_MISSING_UNZ",
                    "Interchange must end with a UNZ segment.",
                    lastSeg.StartOffset,
                    lastSeg.EndOffset,
                    EdiIssueSeverity.Error));
            }
            else
            {
                unzSegment = segments[segments.Count - 1];
            }

            // --- UNH/UNT matching ---
            int messageCount = 0;
            int i = 0;
            while (i < segments.Count)
            {
                if (string.Equals(segments[i].Tag, "UNH", StringComparison.OrdinalIgnoreCase))
                {
                    var unhSegment = segments[i];
                    int unhIndex = i;

                    // Scan forward for matching UNT.
                    int untIndex = -1;
                    for (int j = i + 1; j < segments.Count; j++)
                    {
                        if (string.Equals(segments[j].Tag, "UNH", StringComparison.OrdinalIgnoreCase))
                        {
                            // Hit another UNH before finding UNT — no match.
                            break;
                        }
                        if (string.Equals(segments[j].Tag, "UNT", StringComparison.OrdinalIgnoreCase))
                        {
                            untIndex = j;
                            break;
                        }
                    }

                    if (untIndex < 0)
                    {
                        issues.Add(new EdiValidationIssue(
                            "EDIFACT_MISSING_UNT",
                            $"UNH at offset {unhSegment.StartOffset} has no matching UNT segment.",
                            unhSegment.StartOffset,
                            unhSegment.EndOffset,
                            EdiIssueSeverity.Error));
                        i++;
                        continue;
                    }

                    var untSegment = segments[untIndex];
                    messageCount++;

                    // Count segments from UNH to UNT inclusive.
                    int expectedCount = untIndex - unhIndex + 1;
                    string? untCountStr = GetElementValue(untSegment, 1);
                    if (untCountStr != null && int.TryParse(untCountStr, out int actualCount))
                    {
                        if (actualCount != expectedCount)
                        {
                            issues.Add(new EdiValidationIssue(
                                "EDIFACT_UNT_COUNT_MISMATCH",
                                $"UNT segment count mismatch: declared {actualCount}, actual {expectedCount}.",
                                untSegment.StartOffset,
                                untSegment.EndOffset,
                                EdiIssueSeverity.Error));
                        }
                    }

                    // Compare UNT element 2 with UNH element 1 (message reference number).
                    string? untRef = GetElementValue(untSegment, 2);
                    string? unhRef = GetElementValue(unhSegment, 1);
                    if (untRef != null && unhRef != null
                        && !string.Equals(untRef, unhRef, StringComparison.Ordinal))
                    {
                        issues.Add(new EdiValidationIssue(
                            "EDIFACT_UNT_REF_MISMATCH",
                            $"UNT reference '{untRef}' does not match UNH reference '{unhRef}'.",
                            untSegment.StartOffset,
                            untSegment.EndOffset,
                            EdiIssueSeverity.Error));
                    }

                    i = untIndex + 1;
                }
                else
                {
                    i++;
                }
            }

            // --- UNZ element validation ---
            if (unzSegment != null)
            {
                // UNZ element 1: interchange control count (number of messages).
                string? unzCountStr = GetElementValue(unzSegment, 1);
                if (unzCountStr != null && int.TryParse(unzCountStr, out int declaredMessageCount))
                {
                    if (declaredMessageCount != messageCount)
                    {
                        issues.Add(new EdiValidationIssue(
                            "EDIFACT_UNZ_COUNT_MISMATCH",
                            $"UNZ message count mismatch: declared {declaredMessageCount}, actual {messageCount}.",
                            unzSegment.StartOffset,
                            unzSegment.EndOffset,
                            EdiIssueSeverity.Error));
                    }
                }

                // UNZ element 2 vs UNB element 5 (interchange control reference).
                if (unbSegment != null)
                {
                    string? unzRef = GetElementValue(unzSegment, 2);
                    string? unbRef = GetElementValue(unbSegment, 5);
                    if (unzRef != null && unbRef != null
                        && !string.Equals(unzRef, unbRef, StringComparison.Ordinal))
                    {
                        issues.Add(new EdiValidationIssue(
                            "EDIFACT_UNZ_REF_MISMATCH",
                            $"UNZ reference '{unzRef}' does not match UNB interchange control reference '{unbRef}'.",
                            unzSegment.StartOffset,
                            unzSegment.EndOffset,
                            EdiIssueSeverity.Error));
                    }
                }
            }

            return issues;
        }

        /// <summary>
        /// Gets the raw value of an element at the given 1-based position from a segment.
        /// If the element has components, returns the first component's value.
        /// </summary>
        /// <param name="segment">The segment to read from.</param>
        /// <param name="position">The 1-based element position.</param>
        /// <returns>The element value, or null if not found.</returns>
        private static string? GetElementValue(EdiSegment segment, int position)
        {
            if (segment.Elements == null)
                return null;

            foreach (var element in segment.Elements)
            {
                if (element.Position == position)
                {
                    // If the element has components, return the first component.
                    if (element.Components != null && element.Components.Count > 0)
                        return element.Components[0].RawValue;

                    return element.RawValue;
                }
            }

            return null;
        }
    }
}
