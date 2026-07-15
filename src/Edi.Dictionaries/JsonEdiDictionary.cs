using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Edi.Core.Dictionary;
using Edi.Core.Model;

namespace Edi.Dictionaries
{
    public class JsonEdiDictionary : IEdiDictionary
    {
        private class JsonDictionarySchema
        {
            public string? Standard { get; set; }
            public string? Release { get; set; }
            public Dictionary<string, SegmentDefinition>? Segments { get; set; }
            // Note: qualifiers structure could be changed by the python script.
            // Let's use JsonElement to handle both old phase1 and new schemas if necessary, or just load as objects.
            public JsonElement? Qualifiers { get; set; } 
        }

        private class SegmentDefinition
        {
            public string? Name { get; set; } // python output has 'name'
            public string? Label { get; set; } // phase1 output has 'Label'
            public List<ElementDefinition>? Elements { get; set; }
        }

        private class ElementDefinition
        {
            public int Position { get; set; }
            public string? Id { get; set; }
            public string? Name { get; set; } // python output has 'name'
            public string? Label { get; set; } // phase1 output has 'Label'
            public Dictionary<string, string>? Codes { get; set; }
            public List<ComponentDefinition>? Components { get; set; }
        }

        private class ComponentDefinition
        {
            public int Position { get; set; }
            public string? Id { get; set; }
            public string? Name { get; set; }
        }

        private readonly ConcurrentDictionary<string, JsonDictionarySchema> _cache = new ConcurrentDictionary<string, JsonDictionarySchema>();

        private JsonDictionarySchema? GetSchema(EdiStandard standard, string? release)
        {
            if (string.IsNullOrEmpty(release)) return null;
            
            var key = $"{standard}_{release}".ToLowerInvariant();
            if (_cache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "Data");
            var filename = $"{standard.ToString().ToLowerInvariant()}_{release}.json";
            var filePath = Path.Combine(dir, filename);

            if (!File.Exists(filePath))
            {
                // Cache a null to prevent repeated IO misses? 
                // Let's just return null, or cache an empty schema.
                var empty = new JsonDictionarySchema();
                _cache[key] = empty;
                return empty;
            }

            try
            {
                var jsonString = File.ReadAllText(filePath);
                var schema = JsonSerializer.Deserialize<JsonDictionarySchema>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (schema != null)
                {
                    _cache[key] = schema;
                    return schema;
                }
            }
            catch
            {
                // Ignore parse errors, return null
            }
            
            return null;
        }

        public string? GetSegmentLabel(EdiStandard standard, string? version, string segmentTag)
        {
            var schema = GetSchema(standard, version);
            if (schema?.Segments != null && schema.Segments.TryGetValue(segmentTag, out var segment))
            {
                return segment.Name ?? segment.Label;
            }
            return null;
        }

        public string? GetElementLabel(EdiStandard standard, string? version, string segmentTag, int position)
        {
            var schema = GetSchema(standard, version);
            if (schema?.Segments != null && schema.Segments.TryGetValue(segmentTag, out var segment) && segment.Elements != null)
            {
                foreach (var el in segment.Elements)
                {
                    if (el.Position == position) return el.Name ?? el.Label;
                }
            }
            return null;
        }

        public string? GetComponentLabel(EdiStandard standard, string? version, string segmentTag, int elementPosition, int componentIndex)
        {
            // Note: componentIndex is 0-based in EdiComponent. 
            // The position in JSON is 1-based.
            var schema = GetSchema(standard, version);
            if (schema?.Segments != null && schema.Segments.TryGetValue(segmentTag, out var segment) && segment.Elements != null)
            {
                foreach (var el in segment.Elements)
                {
                    if (el.Position == elementPosition && el.Components != null)
                    {
                        var targetPos = componentIndex + 1;
                        foreach (var comp in el.Components)
                        {
                            if (comp.Position == targetPos) return comp.Name;
                        }
                    }
                }
            }
            return null;
        }

        public string? GetQualifierLabel(EdiStandard standard, string? version, string segmentTag, int elementPosition, string value)
        {
            var schema = GetSchema(standard, version);
            if (schema?.Segments != null && schema.Segments.TryGetValue(segmentTag, out var segment) && segment.Elements != null)
            {
                foreach (var el in segment.Elements)
                {
                    if (el.Position == elementPosition && el.Codes != null)
                    {
                        if (el.Codes.TryGetValue(value, out var desc)) return desc;
                    }
                }
            }
            return null;
        }
    }
}
