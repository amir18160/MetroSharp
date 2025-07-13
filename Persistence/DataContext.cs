using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        /******* Core Logic *******/
        public DbSet<Document> Documents { get; set; }
        public DbSet<OmdbItem> OmdbItems { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<Season> Seasons { get; set; }

        /******* Progress Related *********/

        // add progress and download lated

        /******* User Related *******/
        // no need
        // public new DbSet<User> Users { get; set; }
        public DbSet<BannedUser> BannedUsers { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // 👈 required for IdentityDbContext!

            builder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .IsRequired();

            builder.Entity<Subscription>()
                .HasOne(s => s.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.PlanId)
                .IsRequired();

            builder.Entity<BannedUser>()
                .HasOne(b => b.User)
                .WithOne(u => u.BannedUser)
                .HasForeignKey<BannedUser>(b => b.UserId);
        }

    }
}