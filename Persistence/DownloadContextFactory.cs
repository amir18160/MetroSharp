using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace Persistence
{
    public class DownloadContextFactory : IDesignTimeDbContextFactory<DownloadContext>
    {
        public DownloadContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<DownloadContext>();
            var connectionString = config.GetConnectionString("DownloadContext");

            optionsBuilder.UseSqlite(connectionString); 

            return new DownloadContext(optionsBuilder.Options);
        }
    }
}
