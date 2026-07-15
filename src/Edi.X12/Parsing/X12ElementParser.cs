using System.Collections.Generic;
using Edi.Core.Dictionary;
using Edi.Core.Model;

namespace Edi.X12.Parsing
{
    public static class X12ElementParser
    {
        public static List<EdiElement> ParseElements(string body, char elementSeparator, char componentSeparator, IEdiDictionary? dictionary, string? dictionaryRelease, string tag)
        {
            var elements = new List<EdiElement>();
            if (string.IsNullOrEmpty(body)) return elements;

            string[] rawElements = body.Split(elementSeparator);
            for (int i = 0; i < rawElements.Length; i++)
            {
                int position = i + 1;
                string rawVal = rawElements[i];

                List<EdiComponent>? components = null;
                if (rawVal.IndexOf(componentSeparator) >= 0)
                {
                    components = new List<EdiComponent>();
                    string[] rawComponents = rawVal.Split(componentSeparator);
                    for (int j = 0; j < rawComponents.Length; j++)
                    {
                        int compIndex = j + 1;
                        string? compLabel = dictionary?.GetComponentLabel(EdiStandard.X12, dictionaryRelease, tag, position, compIndex);
                        string? compValueDesc = dictionary?.GetQualifierLabel(EdiStandard.X12, dictionaryRelease, tag, position, rawComponents[j]);
                        components.Add(new EdiComponent(compIndex, rawComponents[j], compLabel, compValueDesc));
                    }
                }

                string? label = dictionary?.GetElementLabel(EdiStandard.X12, dictionaryRelease, tag, position);
                string? elementValueDesc = null;
                if (components == null)
                {
                    elementValueDesc = dictionary?.GetQualifierLabel(EdiStandard.X12, dictionaryRelease, tag, position, rawVal);
                }

                elements.Add(new EdiElement(position, rawVal, label, elementValueDesc, components));
            }

            return elements;
        }
    }
}
