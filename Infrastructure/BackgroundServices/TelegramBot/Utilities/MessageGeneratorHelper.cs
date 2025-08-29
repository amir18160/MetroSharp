using System.Net;
using System.Text;

namespace Infrastructure.BackgroundServices.TelegramBot.Utilities
{
    public static class MessageGeneratorHelper
    {
        public static string E(string s) => s is null ? "" : WebUtility.HtmlEncode(s);
        public static string JoinOrEmpty(IEnumerable<string> arr) => arr == null ? "" : string.Join(", ", arr.Where(x => !string.IsNullOrWhiteSpace(x)));
        public static bool HasValue(string s) => !string.IsNullOrWhiteSpace(s) && s != "N/A";
        public static string FormatLabelWithWrap(string label, string content, bool ExtraNewLine = true)
        {
            if (string.IsNullOrEmpty(content)) return "";
            if (ExtraNewLine)
            {
                return $"{label}:  {content}\n\n";
            }
            return $"{label}:  {content}\n";
        }
        public static void AppendIfNotEmpty(StringBuilder sb, string emoji, string label, string value, bool extraNewLine = true)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                sb.Append(FormatLabelWithWrap($"{emoji} <b>{E(label)}</b>", E(value), extraNewLine));
            }
        }
    }
}