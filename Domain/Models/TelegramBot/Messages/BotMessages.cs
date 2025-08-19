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
        public KeyboardButtons KeyboardButtons { get; set; }
        public CallbackButtons CallbackButtons { get; set; }
        public ErrorMessages ErrorMessages { get; set; }
        public InlineQueryMessages InlineQueryMessages { get; set; }

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
}


