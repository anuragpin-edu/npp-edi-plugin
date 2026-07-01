using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Edi.Core.Dictionary;
using Edi.Core.Model;

namespace Edi.Dictionaries
{
    /// <summary>
    /// An EDI dictionary loaded from an embedded JSON file.
    /// </summary>
    public class JsonEdiDictionary : IEdiDictionary
    {
        private class JsonDictionarySchema
        {
            public string? Standard { get; set; }
            public string? Version { get; set; }
            public string? Provenance { get; set; }
            public Dictionary<string, SegmentDefinition>? Segments { get; set; }
            public Dictionary<string, Dictionary<string, Dictionary<string, string>>>? Qualifiers { get; set; }
        }

        private class SegmentDefinition
        {
            public string? Label { get; set; }
            public Dictionary<string, ElementDefinition>? Elements { get; set; }
        }

        private class ElementDefinition
        {
            public string? Label { get; set; }
            public Dictionary<string, string>? Components { get; set; }
        }

        private readonly JsonDictionarySchema? _data;

        public JsonEdiDictionary()
        {
            var assembly = typeof(JsonEdiDictionary).Assembly;
            var resourceName = "Edi.Dictionaries.edifact-phase1.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                var jsonString = reader.ReadToEnd();
                _data = JsonSerializer.Deserialize<JsonDictionarySchema>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }

        public string? GetSegmentLabel(EdiStandard standard, string segmentTag)
        {
            if (standard != EdiStandard.Edifact || _data?.Segments == null) return null;
            return _data.Segments.TryGetValue(segmentTag, out var segment) ? segment.Label : null;
        }

        public string? GetElementLabel(EdiStandard standard, string segmentTag, int position)
        {
            if (standard != EdiStandard.Edifact || _data?.Segments == null) return null;
            if (_data.Segments.TryGetValue(segmentTag, out var segment) && segment.Elements != null)
            {
                return segment.Elements.TryGetValue(position.ToString(), out var element) ? element.Label : null;
            }
            return null;
        }

        public string? GetComponentLabel(EdiStandard standard, string segmentTag, int elementPosition, int componentIndex)
        {
            if (standard != EdiStandard.Edifact || _data?.Segments == null) return null;
            if (_data.Segments.TryGetValue(segmentTag, out var segment) && segment.Elements != null)
            {
                if (segment.Elements.TryGetValue(elementPosition.ToString(), out var element) && element.Components != null)
                {
                    return element.Components.TryGetValue(componentIndex.ToString(), out var componentLabel) ? componentLabel : null;
                }
            }
            return null;
        }

        public string? GetQualifierLabel(EdiStandard standard, string segmentTag, int elementPosition, string value)
        {
            if (standard != EdiStandard.Edifact || _data?.Qualifiers == null) return null;
            if (_data.Qualifiers.TryGetValue(segmentTag, out var elementQualifiers))
            {
                if (elementQualifiers.TryGetValue(elementPosition.ToString(), out var codeList))
                {
                    return codeList.TryGetValue(value, out var label) ? label : null;
                }
            }
            return null;
        }
    }
}
