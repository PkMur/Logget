using LogGet.Models;
using Microsoft.AspNetCore.Identity;

namespace LogGet.Services;

public class UserAuthenticationService : IUserService
{
    private readonly IUsuarioService _usuarioService;
    private readonly PasswordHasher<object> _hasher = new();

    public UserAuthenticationService(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public ApplicationUser? ValidateCredentials(string login, string senha)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(senha)) return null;

        var user = _usuarioService.ListAll().FirstOrDefault(u => string.Equals(u.Login, login, StringComparison.OrdinalIgnoreCase));
        if (user is null) return null;
    // Não permitir que usuários inativos se autentiquem
        if (!user.IsActive) return null;

        var storedHash = user.Senha ?? string.Empty;
        var result = _hasher.VerifyHashedPassword(null, storedHash, senha);
        if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            return new ApplicationUser
            {
                Id = user.Id,
                Login = user.Login,
                Name = user.Nome,
                Roles = Array.Empty<string>()
            };
        }

        return null;
    }
}
