using System;
using Edi.Dictionaries;
using Edi.Core.Model;

class Program {
    static void Main() {
        var dict = new JsonEdiDictionary();
        var label = dict.GetSegmentLabel(EdiStandard.Edifact, "D96A", "BGM");
        Console.WriteLine("BGM: " + label);
    }
}
