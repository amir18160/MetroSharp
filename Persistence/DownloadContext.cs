using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class DownloadContext : DbContext
    {
        public DownloadContext(DbContextOptions<DownloadContext> options) : base(options) { }

        public DbSet<ApiUsage> ApiUsages { get; set; }
        public DbSet<TorrentTask> TorrentTasks { get; set; }
        public DbSet<TaskDownloadProgress> TaskDownloadProgress { get; set; }
        public DbSet<SubtitleVideoPair> SubtitleVideoPairs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApiUsage>()
                .HasKey(u => new { u.ApiKey, u.Date });

            builder.Entity<TorrentTask>()
                .HasOne(t => t.TaskDownloadProgress)
                .WithOne(t => t.TorrentTask)
                .HasForeignKey<TaskDownloadProgress>(t => t.TorrentTaskId);

            builder.Entity<TorrentTask>()
                .HasMany(t => t.SubtitleVideoPairs)
                .WithOne(t => t.TorrentTask)
                .HasForeignKey(t => t.TorrentTaskId);
        }
    }
}