namespace Infrastructure.BackgroundServices.TelegramBot.Configs;

public class TelegramBotSettings
{
    public bool IsEnabled { get; set; }
    public int AppId { get; set; }
    public string ApiHash { get; set; }
    public string BotToken { get; set; }
    public string BotUsername { get; set; }
    public string DevelopmentBotToken { get; set; }
    public string MailChannelUsername { get; set; }
    public long MainChannelID { get; set; }
    public long ChannelFileChatID { get; set; }
    public long MaxAllowedUploadSize { get; set; }
    public string LogPath { get; set; }
    public long BinChannelChatID { get; set; }
    public long ForwardChannelID { get; set; }
    public long ForwardChannelPeerID { get; set; }
    public string BuyCoffeeURL { get; set; }
    public string RaymitURL { get; set; }
}