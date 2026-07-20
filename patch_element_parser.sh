#!/bin/bash
sed -i '' 's/IEdiDictionary? dictionary,/IEdiDictionary? dictionary, string? version,/g' src/Edi.Edifact/Parsing/EdifactElementParser.cs
sed -i '' 's/IEdiDictionary? dictionary, EdiStandard standard/IEdiDictionary? dictionary, string? version, EdiStandard standard/g' src/Edi.X12/Parsing/X12ElementParser.cs
sed -i '' 's/dictionary?.GetElementLabel(standard, segmentTag, elementPos)/dictionary?.GetElementLabel(standard, version, segmentTag, elementPos)/g' src/Edi.Edifact/Parsing/EdifactElementParser.cs
sed -i '' 's/dictionary?.GetComponentLabel(standard, segmentTag, elementPos, compIndex)/dictionary?.GetComponentLabel(standard, version, segmentTag, elementPos, compIndex)/g' src/Edi.Edifact/Parsing/EdifactElementParser.cs
sed -i '' 's/dictionary?.GetQualifierLabel(standard, segmentTag, elementPos, compVal)/dictionary?.GetQualifierLabel(standard, version, segmentTag, elementPos, compVal)/g' src/Edi.Edifact/Parsing/EdifactElementParser.cs
sed -i '' 's/dictionary?.GetElementLabel(standard, segmentTag, elementPos)/dictionary?.GetElementLabel(standard, version, segmentTag, elementPos)/g' src/Edi.X12/Parsing/X12ElementParser.cs
sed -i '' 's/dictionary?.GetComponentLabel(standard, segmentTag, elementPos, compIndex)/dictionary?.GetComponentLabel(standard, version, segmentTag, elementPos, compIndex)/g' src/Edi.X12/Parsing/X12ElementParser.cs
sed -i '' 's/dictionary?.GetQualifierLabel(standard, segmentTag, elementPos, compVal)/dictionary?.GetQualifierLabel(standard, version, segmentTag, elementPos, compVal)/g' src/Edi.X12/Parsing/X12ElementParser.cs
