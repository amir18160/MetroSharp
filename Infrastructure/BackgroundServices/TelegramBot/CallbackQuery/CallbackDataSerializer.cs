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
            Season,
            Episode,
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
            if (callbackData.StartsWith("season_"))
                return CallbackType.Season;
            if (callbackData.StartsWith("episode_"))
                return CallbackType.Episode;

            return CallbackType.Unknown;
        }

        /* parse is required */

        public static string CreateDownloadFiles(string imdbId) => $"dl_{imdbId}";
        public static string ParseDownloadFilesParams(string callbackData) => ParseParams(callbackData, "dl_");

        public static string CreateGetDocumentParams(string documentId) => $"doc_{documentId}";
        public static string ParseDocumentFileParams(string callbackData) => ParseParams(callbackData, "doc_");

        public static string CreateGetSeasonParams(string documentId) => $"season_{documentId}";
        public static string ParseSeasonParams(string callbackData) => ParseParams(callbackData, "season_");

        public static string CreateEpisodeParams(string documentId) => $"episode_{documentId}";
        public static string ParseEpisodeParams(string callbackData) => ParseParams(callbackData, "episode_");

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