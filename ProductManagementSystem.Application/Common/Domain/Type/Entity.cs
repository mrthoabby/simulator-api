namespace ProductManagementSystem.Application.Common.Domain.Type;

public class Entity
{
    public string Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    protected Entity()
    {
        Id = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

}