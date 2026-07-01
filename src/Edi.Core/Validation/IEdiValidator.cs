using System.Collections.Generic;
using Edi.Core.Model;

namespace Edi.Core.Validation
{
    /// <summary>
    /// Defines methods for validating an EDI document.
    /// </summary>
    public interface IEdiValidator
    {
        IReadOnlyList<EdiValidationIssue> Validate(EdiDocument document);
    }
}
