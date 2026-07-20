using System;
using System.Text;
using Edi.Core.Formatting;
using Edi.Core.Model;
using Edi.X12.Detection;

namespace Edi.X12.Formatting
{
    public sealed class X12Formatter : IEdiFormatter
    {
        public bool CanFormat(string text)
        {
            return X12DelimiterDetector.IsX12(text);
        }

        public string Prettify(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrWhiteSpace(text)) return text;
            if (!CanFormat(text)) return text;

            EdiDelimiters delimiters = X12DelimiterDetector.Detect(text);
            var sb = new StringBuilder(text.Length + 64);
            char terminator = delimiters.SegmentTerminator;

            int start = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == terminator)
                {
                    string trimmed = text.Substring(start, i - start).Trim('\r', '\n', ' ', '\t');
                    if (trimmed.Length > 0)
                    {
                        sb.Append(trimmed);
                        sb.Append(terminator);
                        sb.AppendLine();
                    }
                    start = i + 1;
                }
            }

            if (start < text.Length)
            {
                string trimmed = text.Substring(start).Trim('\r', '\n', ' ', '\t');
                if (trimmed.Length > 0)
                {
                    sb.Append(trimmed);
                    sb.AppendLine();
                }
            }

            // Remove trailing extra newlines if needed, then add exactly one.
            while (sb.Length > 0)
            {
                char last = sb[sb.Length - 1];
                if (last == '\n' || last == '\r' || last == ' ')
                    sb.Length--;
                else
                    break;
            }
            sb.AppendLine();

            return sb.ToString();
        }

        public string Minify(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrWhiteSpace(text)) return text;
            if (!CanFormat(text)) return text;

            EdiDelimiters delimiters = X12DelimiterDetector.Detect(text);
            var sb = new StringBuilder(text.Length);
            char terminator = delimiters.SegmentTerminator;

            int start = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == terminator)
                {
                    string trimmed = text.Substring(start, i - start).Trim('\r', '\n', ' ', '\t');
                    if (trimmed.Length > 0)
                    {
                        sb.Append(trimmed);
                        sb.Append(terminator);
                    }
                    start = i + 1;
                }
            }

            if (start < text.Length)
            {
                string trimmed = text.Substring(start).Trim('\r', '\n', ' ', '\t');
                if (trimmed.Length > 0)
                {
                    sb.Append(trimmed);
                }
            }

            return sb.ToString();
        }
    }
}
