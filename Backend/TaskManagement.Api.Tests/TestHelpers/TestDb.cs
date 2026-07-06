using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Domain;
using TaskManagement.Api.Infrastructure;

namespace TaskManagement.Api.Tests.TestHelpers;

/// <summary>
/// SQLite in-memory database for handler tests. The connection must stay open
/// for the lifetime of the test, otherwise the in-memory database is dropped.
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Context { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    public User AddUser(string email = "user@test.local")
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "irrelevant",
            CreatedDate = DateTime.UtcNow
        };
        Context.Users.Add(user);
        Context.SaveChanges();
        return user;
    }

    public TaskItem AddTask(Guid userId, int order, string title = "Task", Priority priority = Priority.Medium, DateTime? completedDate = null)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Order = order,
            Priority = priority,
            Title = title,
            Description = $"{title} description",
            CreatedDate = DateTime.UtcNow,
            CompletedDate = completedDate,
            CompletedDescription = completedDate is null ? null : "done"
        };
        Context.Tasks.Add(task);
        Context.SaveChanges();
        return task;
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
