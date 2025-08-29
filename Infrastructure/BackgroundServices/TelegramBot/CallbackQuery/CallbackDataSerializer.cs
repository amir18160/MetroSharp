using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.BackgroundServices.TelegramBot.CallbackQuery
{
    public static class CallbackDataSerializer
    {
        public enum CallbackType
        {
            Download,
            Document,
            RecommendedTitles,
            NewTitles,
            Unknown
        }

        public static CallbackType GetCallbackType(TL.UpdateBotCallbackQuery callbackQuery)
        {
            var callbackData = Encoding.UTF8.GetString(callbackQuery.data);

            if (callbackData.StartsWith("dl_"))
                return CallbackType.Download;
            if (callbackData.StartsWith("doc_"))
                return CallbackType.Document;
            if (callbackData.StartsWith("rcmt_"))
                return CallbackType.RecommendedTitles;
            if (callbackData.StartsWith("newt_"))
                return CallbackType.NewTitles;

            return CallbackType.Unknown;
        }

        /* parse is required */

        public static string CreateDownloadFiles(string imdbId) => $"dl_{imdbId}";
        public static string ParseDownloadFilesParams(string callbackData) => ParseParams(callbackData, "dl_");

        public static string CreateGetDocumentParams(string documentId) => $"doc_{documentId}";
        public static string ParseDocumentFileParams(string callbackData) => ParseParams(callbackData, "doc_");

        /* no parse required */
        public static string CreateRecommendedTitlesParams() => $"rcmt_";
        public static string CreateNewTitlesParams() => $"newt_";


        /* parser */
        private static string ParseParams(string callbackData, string prefix)
        {
            var match = Regex.Match(callbackData, $@"{Regex.Escape(prefix)}(.+)");
            if (match.Success)
                return match.Groups[1].Value;

            throw new Exception($"Data is invalid! Failed to parse value from callbackData with prefix '{prefix}'");
        }
    }
}
