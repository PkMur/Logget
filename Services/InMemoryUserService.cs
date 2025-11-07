using LogGet.Models;

namespace LogGet.Services;

public class InMemoryUserService : IUserService
{
    private readonly List<UserRecord> _users = new()
    {
        new UserRecord
        {
            Id = Guid.NewGuid(),
            Login = "admin",
            Password = "123456",
            Name = "Administrador",
            Roles = new[] { "Administrador" }
        }
    };

    public ApplicationUser? ValidateCredentials(string login, string senha)
    {
        var user = _users.FirstOrDefault(u =>
            string.Equals(u.Login, login, StringComparison.OrdinalIgnoreCase) &&
            u.Password == senha);

        if (user is null)
        {
            return null;
        }

        return new ApplicationUser
        {
            Id = user.Id,
            Login = user.Login,
            Name = user.Name,
            Roles = user.Roles
        };
    }

    private sealed class UserRecord
    {
        public Guid Id { get; init; }
        public string Login { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
    }
}

