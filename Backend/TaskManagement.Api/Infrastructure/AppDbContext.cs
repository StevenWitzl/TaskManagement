using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskManagement.Api.Domain;

namespace TaskManagement.Api.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite has no DateTime type; mark round-tripped values as UTC so they serialize with the Z suffix.
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>()
            .HaveConversion<NullableUtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.Id);
            user.Property(u => u.Email).IsRequired().HasMaxLength(256);
            user.HasIndex(u => u.Email).IsUnique();
            user.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            user.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            user.Property(u => u.PasswordHash).IsRequired();
            user.Property(u => u.CreatedDate).IsRequired();
        });

        modelBuilder.Entity<TaskItem>(task =>
        {
            task.HasKey(t => t.Id);
            task.Property(t => t.Order).IsRequired();
            task.Property(t => t.Priority).IsRequired();
            task.Property(t => t.Title).IsRequired().HasMaxLength(200);
            task.Property(t => t.Description).IsRequired().HasMaxLength(2000);
            task.Property(t => t.CreatedDate).IsRequired();
            task.Property(t => t.CompletedDescription).HasMaxLength(2000);

            task.HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
