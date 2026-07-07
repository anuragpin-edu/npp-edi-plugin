namespace Edi.Core.Formatting
{
    /// <summary>
    /// Defines methods for formatting EDI text.
    /// </summary>
    public interface IEdiFormatter
    {
        /// <summary>
        /// Determines whether this formatter can handle the given EDI text.
        /// </summary>
        /// <param name="text">The raw EDI text to inspect.</param>
        /// <returns><c>true</c> if this formatter supports the text's standard; otherwise <c>false</c>.</returns>
        bool CanFormat(string text);

        /// <summary>
        /// Formats the EDI text so each segment appears on its own line.
        /// </summary>
        /// <param name="text">The raw EDI text.</param>
        /// <returns>The prettified EDI text.</returns>
        string Prettify(string text);

        /// <summary>
        /// Minifies the EDI text into a compact format with no whitespace between segments.
        /// </summary>
        /// <param name="text">The raw EDI text.</param>
        /// <returns>The minified EDI text.</returns>
        string Minify(string text);
    }
}
