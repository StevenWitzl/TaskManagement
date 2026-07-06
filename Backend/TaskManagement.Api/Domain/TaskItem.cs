namespace TaskManagement.Api.Domain;

public class TaskItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Order { get; set; }
    public Priority Priority { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? CompletedDescription { get; set; }

    public User? User { get; set; }
}
