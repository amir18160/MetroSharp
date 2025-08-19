using System.Text;

namespace Infrastructure.BackgroundServices.TelegramBot.Configs
{
    public class TelegramLogger
    {
        public static void CreateLogger(string logPath)
        {
            var directory = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var stream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter WTelegramLogs = new StreamWriter(stream, Encoding.UTF8)
            {
                AutoFlush = true
            };

            WTelegram.Helpers.Log = (lvl, str) =>
                WTelegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");
        }
    }
}