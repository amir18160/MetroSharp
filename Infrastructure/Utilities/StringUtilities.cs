using System.Reflection.Metadata;
using Domain.Core;

namespace Infrastructure.Utilities
{
    public static class StringUtilities
    {
        public static string TruncateAtWord(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            if (text.Length <= maxLength)
                return text;

            int lastSpace = text.LastIndexOf(' ', maxLength);
            if (lastSpace <= 0) lastSpace = maxLength;

            return string.Concat(text.AsSpan(0, lastSpace), "...");
        }
        
        public static string GetIMDbURL(string imdbId)
        {
            return $"{Constants.IMDbTitleBase}{imdbId}";
        }
    }
}