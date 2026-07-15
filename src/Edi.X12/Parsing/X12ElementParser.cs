using System.Collections.Generic;
using Edi.Core.Dictionary;
using Edi.Core.Model;

namespace Edi.X12.Parsing
{
    public static class X12ElementParser
    {
        public static List<EdiElement> ParseElements(string body, EdiDelimiters delimiters, IEdiDictionary? dictionary, string tag)
        {
            var elements = new List<EdiElement>();
            if (string.IsNullOrEmpty(body)) return elements;

            string[] rawElements = body.Split(delimiters.ElementSeparator);
            for (int i = 0; i < rawElements.Length; i++)
            {
                int position = i + 1;
                string rawVal = rawElements[i];

                List<EdiComponent>? components = null;
                if (rawVal.IndexOf(delimiters.ComponentSeparator) >= 0)
                {
                    components = new List<EdiComponent>();
                    string[] rawComponents = rawVal.Split(delimiters.ComponentSeparator);
                    for (int j = 0; j < rawComponents.Length; j++)
                    {
                        int compIndex = j + 1;
                        string? compLabel = dictionary?.GetComponentLabel(EdiStandard.X12, tag, position, compIndex);
                        components.Add(new EdiComponent(compIndex, rawComponents[j], compLabel));
                    }
                }

                string? label = dictionary?.GetElementLabel(EdiStandard.X12, tag, position);
                elements.Add(new EdiElement(position, rawVal, label, components));
            }

            return elements;
        }
    }
}
