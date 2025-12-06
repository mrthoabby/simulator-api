namespace ProductManagementSystem.Application.AppEntities.Users.Models;

public class Company
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;

    public static Company Create(string name)
    {
        return new Company { Id = Guid.NewGuid().ToString(), Name = name };
    }
}
