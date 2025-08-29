using System.Runtime.CompilerServices;

namespace Domain.Models.TelegramBot.Messages
{
    public class BotMessages
    {
        public string WelcomeMessage { get; set; }
        public string FarewellMessage { get; set; }
        public string UnSupportedCommand { get; set; }
        public string LatestMoviesMessage { get; set; }
        public string AboutUs { get; set; }
        public string SearchTitles { get; set; }
        public string SupportUs { get; set; }
        public Common Common { get; set; }
        public KeyboardButtons KeyboardButtons { get; set; }
        public CallbackButtons CallbackButtons { get; set; }
        public ErrorMessages ErrorMessages { get; set; }
        public InlineQueryMessages InlineQueryMessages { get; set; }
        public InlineQuerySearchResponse InlineQuerySearchResponse { get; set; }
        public CallbackQueryResponse CallbackQueryResponse { get; set; }


    }
    public class KeyboardButtons
    {
        public string AboutUs { get; set; }
        public string SupportUs { get; set; }
        public string LatestTitles { get; set; }
        public string SearchTitles { get; set; }
    }
    public class CallbackButtons
    {
        public string Search { get; set; }
        public string Download { get; set; }
        public string MoreInfo { get; set; }
        public string RecommendedTitles { get; set; }
        public string NewPopularTitles { get; set; }
    }
    public class CallbackQueryResponse
    {
        public string IsSoftSubbed { get; set; }
        public string Size { get; set; }
        public string Resolution { get; set; }
        public string Codec { get; set; }
        public string Encoder { get; set; }
        public string FileName { get; set; }
        public string SelectDocument { get; set; }

    }
    public class ErrorMessages
    {
        public string Unknown { get; set; }
        public string NotImplemented { get; set; }
    }

    public class InlineQueryMessages
    {
        public string TypeToSearch { get; set; }
        public string TypeToSearchContent { get; set; }
    }

    public class InlineQuerySearchResponse
    {
        public string Title { get; set; }
        public Scores Scores { get; set; }
        public string Actors { get; set; }
        public string Directors { get; set; }
        public string Writers { get; set; }
        public string Countries { get; set; }
        public string Languages { get; set; }
        public string Plot { get; set; }
        public string BoxOffice { get; set; }
        public string Awards { get; set; }
        public string Genres { get; set; }
        public string NotFoundMessage { get; set; }
        public string Poster { get; set; }
        public string Rated { get; set; }
        public string Released { get; set; }
        public string Runtime { get; set; }
        public string Information { get; set; }
    }

    public class Scores
    {
        public string Score { get; set; }
        public string Imdb { get; set; }
        public string Rotten { get; set; }
        public string Metascore { get; set; }
    }

    public class Common
    {
        public string Movie { get; set; }
        public string Series { get; set; }
        public string Share { get; set; }
        public string Yes { get; set; }
        public string No { get; set; }
    }
}


