using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Domain;

namespace TaskManagement.Api.Infrastructure;

public static class DbSeeder
{
    public const string DemoEmail = "demo@taskmanagement.local";
    public const string DemoPassword = "Demo123!";

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher passwordHasher, ILogger logger)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync())
        {
            logger.LogInformation("Database already seeded, skipping.");
            return;
        }

        logger.LogInformation("Seeding database with demo user and tasks...");

        var demoUser = new User
        {
            Id = Guid.NewGuid(),
            Email = DemoEmail,
            FirstName = "Demo",
            LastName = "User",
            PasswordHash = passwordHasher.Hash(DemoPassword),
            CreatedDate = DateTime.UtcNow
        };

        var tasks = new[]
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = demoUser.Id,
                Order = 1,
                Priority = Priority.High,
                Title = "Review pull requests",
                Description = "Go through the open pull requests and leave feedback.",
                CreatedDate = DateTime.UtcNow.AddDays(-3)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = demoUser.Id,
                Order = 2,
                Priority = Priority.Medium,
                Title = "Write sprint summary",
                Description = "Summarize what shipped this sprint for the team update.",
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = demoUser.Id,
                Order = 3,
                Priority = Priority.Low,
                Title = "Clean up backlog",
                Description = "Archive stale tickets and re-prioritize the rest.",
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = demoUser.Id,
                Order = 4,
                Priority = Priority.High,
                Title = "Set up CI pipeline",
                Description = "Add a build-and-test pipeline for the new repository.",
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                CompletedDate = DateTime.UtcNow.AddDays(-1),
                CompletedDescription = "Pipeline runs build + unit tests on every push."
            }
        };

        db.Users.Add(demoUser);
        db.Tasks.AddRange(tasks);
        await db.SaveChangesAsync();

        logger.LogInformation("Seeded demo user {Email} with {Count} tasks.", DemoEmail, tasks.Length);
    }
}
