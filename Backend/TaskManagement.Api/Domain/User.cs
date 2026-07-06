namespace TaskManagement.Api.Domain;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
