using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class DataContext : IdentityDbContext<User>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    /******* Core Logic *******/
    public DbSet<Document> Documents { get; set; }
    public DbSet<OmdbItem> OmdbItems { get; set; }
    public DbSet<Episode> Episodes { get; set; }
    public DbSet<Season> Seasons { get; set; }
    public DbSet<Tag> Tags { get; set; }


    /******* User Related *******/
    public DbSet<EmailVerification> EmailVerifications { get; set; }
    public DbSet<BannedUser> BannedUsers { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<DocumentUsage> DocumentUsages { get; set; }
    public DbSet<MediaRequest> MediaRequests { get; set; }
    
    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Subscription <-> User (one-to-many)
        builder.Entity<Subscription>()
            .HasOne(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.UserId)
            .IsRequired();
        
        // Subscription <-> Plan (one-to-many)
        builder.Entity<Subscription>()
            .HasOne(s => s.Plan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.PlanId)
            .IsRequired();

        // BannedUser <-> User (one-to-one)
        builder.Entity<BannedUser>()
            .HasOne(b => b.User)
            .WithOne(u => u.BannedUser)
            .HasForeignKey<BannedUser>(b => b.UserId);

        // EmailVerification <-> User (one-to-many)
        builder.Entity<EmailVerification>()
            .HasOne(ev => ev.User)
            .WithMany(u => u.EmailVerifications)
            .HasForeignKey(ev => ev.UserId);

        // DocumentUsage <-> Document (one-to-many)
        builder.Entity<DocumentUsage>()
            .HasOne(ev => ev.Document)
            .WithMany(d => d.DocumentUsages)
            .HasForeignKey(ev => ev.DocumentId);
        
        // DocumentUsage <-> User (one-to-many)
        builder.Entity<DocumentUsage>()
            .HasOne(ev => ev.User)
            .WithMany(d => d.DocumentUsages)
            .HasForeignKey(ev => ev.UserId);
        
        // MediaRequest-Creator <-> User (one-to-many)
        builder.Entity<MediaRequest>()
            .HasOne(mr => mr.CreatorUser)
            .WithMany(d => d.CreatedRequests)
            .HasForeignKey(ev => ev.CreatorUserId);
        
        // MediaRequest-Responser <-> User (one-to-many)
        builder.Entity<MediaRequest>()
            .HasOne(mr => mr.ResponserUser)
            .WithMany(d => d.RespondedRequests)
            .HasForeignKey(ev => ev.ResponserUser);

        // OMDb Item <-> Tag (one-to-many)
        builder.Entity<OmdbItem>()
            .HasMany(item => item.Tags)
            .WithOne(tag => tag.OmdbItem)
            .HasForeignKey(tag => tag.OmdbItemId);
        
    }
}