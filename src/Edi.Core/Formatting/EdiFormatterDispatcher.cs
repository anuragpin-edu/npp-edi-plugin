using System.Collections.Generic;
using System.Linq;

namespace Edi.Core.Formatting
{
    /// <summary>
    /// Dispatches formatting requests to the first registered formatter that can handle the text.
    /// Mirrors the design of <see cref="Edi.Core.Parsing.EdiParserDispatcher"/>.
    /// </summary>
    public sealed class EdiFormatterDispatcher
    {
        private readonly List<IEdiFormatter> _formatters;

        /// <summary>
        /// Initializes a new instance with the given formatters, checked in registration order.
        /// </summary>
        /// <param name="formatters">The formatters to dispatch to.</param>
        public EdiFormatterDispatcher(IEnumerable<IEdiFormatter> formatters)
        {
            _formatters = formatters?.ToList() ?? new List<IEdiFormatter>();
        }

        /// <summary>
        /// Prettifies the EDI text using the first matching formatter.
        /// </summary>
        /// <param name="text">The raw EDI text.</param>
        /// <returns>A <see cref="FormatResult"/> indicating success or failure.</returns>
        public FormatResult Prettify(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return FormatResult.Failure("No text to format.");
            }

            foreach (var formatter in _formatters)
            {
                if (formatter.CanFormat(text))
                {
                    string result = formatter.Prettify(text);
                    return FormatResult.Success(result);
                }
            }

            return FormatResult.Failure("Unsupported or unrecognised EDI standard.");
        }

        /// <summary>
        /// Minifies the EDI text using the first matching formatter.
        /// </summary>
        /// <param name="text">The raw EDI text.</param>
        /// <returns>A <see cref="FormatResult"/> indicating success or failure.</returns>
        public FormatResult Minify(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return FormatResult.Failure("No text to format.");
            }

            foreach (var formatter in _formatters)
            {
                if (formatter.CanFormat(text))
                {
                    string result = formatter.Minify(text);
                    return FormatResult.Success(result);
                }
            }

            return FormatResult.Failure("Unsupported or unrecognised EDI standard.");
        }
    }

    /// <summary>
    /// Represents the result of a formatting operation.
    /// </summary>
    public sealed class FormatResult
    {
        /// <summary>Gets whether the formatting succeeded.</summary>
        public bool IsSuccess { get; }

        /// <summary>Gets the formatted text, or null if formatting failed.</summary>
        public string? FormattedText { get; }

        /// <summary>Gets an error message when formatting failed, or null on success.</summary>
        public string? ErrorMessage { get; }

        private FormatResult(bool isSuccess, string? formattedText, string? errorMessage)
        {
            IsSuccess = isSuccess;
            FormattedText = formattedText;
            ErrorMessage = errorMessage;
        }

        /// <summary>Creates a successful result with the formatted text.</summary>
        public static FormatResult Success(string formattedText)
        {
            return new FormatResult(true, formattedText, null);
        }

        /// <summary>Creates a failure result with the given error message.</summary>
        public static FormatResult Failure(string errorMessage)
        {
            return new FormatResult(false, null, errorMessage);
        }
    }
}
