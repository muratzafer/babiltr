using babiltr.EntityLayer;
using EntityLayer.Concrete;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, Role, int>(options)
{
    public DbSet<Jobs> Jobs { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Skills> Skills { get; set; }
    public DbSet<Education> Educations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Translation> Translations { get; set; }   
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Commission> Commissions { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<BlogContent> BlogContents { get; set; }

    // YORUM SATIRI ALTINDA: EKLENEN KOD BUDUR
    public DbSet<EntityLayer.Concrete.Payment> Payments { get; set; }
    public DbSet<EntityLayer.Concrete.Payout>  Payouts  { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Jobs>()
            .HasKey(j => j.JobID);

        modelBuilder.Entity<Application>()
            .HasOne(a => a.User)
            .WithMany(u => u.Applications)
            .HasForeignKey(a => a.UserID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Chat>()
            .HasOne(c => c.User1)
            .WithMany()
            .HasForeignKey(c => c.User1Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Chat>()
            .HasOne(c => c.User2)
            .WithMany()
            .HasForeignKey(c => c.User2Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Chat)
            .WithMany()
            .HasForeignKey(m => m.ChatId);

        modelBuilder.Entity<Translation>()
            .HasOne(t => t.Job)
            .WithMany(j => j.Translations)
            .HasForeignKey(t => t.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Application>()
            .HasOne(a => a.Job)
            .WithMany(j => j.Applications)
            .HasForeignKey(a => a.JobID);

        modelBuilder.Entity<Blog>()
            .HasMany(b => b.BlogContents)
            .WithOne(bc => bc.Blog)
            .HasForeignKey(bc => bc.BlogId);

        // --- Payments / Payouts relations ---
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Job)
            .WithMany() // no collection on Jobs entity
            .HasForeignKey(p => p.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payout>()
            .HasOne(po => po.Payment)
            .WithMany() // no collection on Payment entity
            .HasForeignKey(po => po.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}