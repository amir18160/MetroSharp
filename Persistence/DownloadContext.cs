using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class DownloadContext : DbContext
    {
        public DownloadContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<ApiUsage> ApiUsages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApiUsage>()
                .HasKey(u => new { u.ApiKey, u.Date });
        }
    }
}