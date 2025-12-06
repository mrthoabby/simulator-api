using FluentValidation;

namespace ProductManagementSystem.Application.AppEntities.Users.Models;

public class User
{
    public string Id { get; init; }
    public string Name { get; init; }
    public Credential Credential { get; private set; }

    private User(string name, Credential credential)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Credential = credential;
    }


    public static User Create(string name, Credential credential)
    {
        return new User(name, credential);
    }


}

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("User name is required")
            .MaximumLength(100).WithMessage("User name cannot exceed 100 characters");

        RuleFor(x => x.Credential)
            .NotNull().WithMessage("Credential is required");

    }
}