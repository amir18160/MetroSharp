namespace Infrastructure.BackgroundServices.DatabaseBackupService
{
    public class BackupSettings
    {
        public bool IsEnabled { get; set; }
        public long ChannelId { get; set; }
        public int BackupPeriod { get; set; }
        public List<string> BackupPaths { get; set; }
    }
}