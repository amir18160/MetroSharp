namespace Infrastructure.BackgroundServices.Models
{
    public class TelegramBotSettings
    {
        public int AppId { get; set; }
        public string ApiHash { get; set; }
        public string BotToken { get; set; }
        public string DevelopmentBotToken { get; set; }
        public string TelegramContext { get; set; }
        public long ChannelFileChatID { get; set; }
        public long MaxAllowedUploadSize { get; set; }
    }
}