namespace Edi.Core.Formatting
{
    /// <summary>
    /// Defines methods for formatting EDI text.
    /// </summary>
    public interface IEdiFormatter
    {
        string Prettify(string text);
        string Minify(string text);
    }
}
