using LogGet.Models;
using Microsoft.AspNetCore.Identity;

namespace LogGet.Services;

public class InMemoryUserService : IUserService
{
    private readonly List<UserRecord> _users;
    private readonly PasswordHasher<object> _hasher = new();

    public InMemoryUserService()
    {
        _users = new List<UserRecord>
        {
            new UserRecord
            {
                Id = Guid.NewGuid().ToString(),
                Login = "admin",
                // Armazena a senha padrão já hasheada
                Password = _hasher.HashPassword(null, "123456"),
                Name = "Administrador",
                Roles = new[] { "Administrador" }
            }
        };
    }

    public ApplicationUser? ValidateCredentials(string login, string senha)
    {
        var user = _users.FirstOrDefault(u =>
            string.Equals(u.Login, login, StringComparison.OrdinalIgnoreCase));

        if (user is null) return null;

        var verified = _hasher.VerifyHashedPassword(null, user.Password, senha);
        if (verified != PasswordVerificationResult.Success && verified != PasswordVerificationResult.SuccessRehashNeeded)
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
        public string Id { get; init; } = string.Empty;
        public string Login { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
    }
}

