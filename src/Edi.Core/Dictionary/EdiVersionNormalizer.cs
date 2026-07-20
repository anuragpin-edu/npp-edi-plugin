using System;
using Edi.Core.Model;

namespace Edi.Core.Dictionary
{
    public static class EdiVersionNormalizer
    {
        public static string Normalize(EdiStandard standard, string? rawVersion)
        {
            if (rawVersion == null || string.IsNullOrWhiteSpace(rawVersion))
                return string.Empty;

            rawVersion = rawVersion.Trim();

            if (standard == EdiStandard.X12)
            {
                // X12 expected forms: 004010, 005010, 004010X092A1, 00501
                var match = System.Text.RegularExpressions.Regex.Match(rawVersion, @"^00\d{3}");
                if (match.Success)
                {
                    return match.Value;
                }
                return string.Empty;
            }
            else if (standard == EdiStandard.Edifact)
            {
                // EDIFACT expected forms: D:96A, D96A, D:01B, D01B
                if (rawVersion.Contains(":"))
                {
                    return rawVersion.Replace(":", "");
                }
            }

            return rawVersion;
        }
    }
}
