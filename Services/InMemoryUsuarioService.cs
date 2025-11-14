using LogGet.Models;
using Microsoft.AspNetCore.Identity;

namespace LogGet.Services;

public class InMemoryUsuarioService : IUsuarioService
{
    private readonly List<UsuarioViewModel> _users = new();
    private readonly PasswordHasher<object> _hasher = new();

    public InMemoryUsuarioService()
    {
    // Criar usuário administrador padrão para conveniência de desenvolvimento (login: admin / senha: 123456)
        var admin = new UsuarioViewModel
        {
            Id = Guid.NewGuid().ToString(),
            Nome = "Administrador",
            CPF = string.Empty,
            RG = string.Empty,
            Email = "admin@local",
            Login = "admin",
            Senha = _hasher.HashPassword(null, "123456"),
            Telefone = string.Empty,
            IsActive = true
        };

        _users.Add(admin);
    }

    public IEnumerable<UsuarioViewModel> ListAll(string? query = null)
    {
        IEnumerable<UsuarioViewModel> data = _users.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.Trim();
            data = data.Where(u =>
                u.Nome.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                u.Login.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        return data;
    }

    public void Add(UsuarioViewModel usuario)
    {
    // Garantir que a senha seja armazenada hasheada
        if (!string.IsNullOrEmpty(usuario.Senha))
        {
            usuario.Senha = _hasher.HashPassword(null, usuario.Senha);
        }

        if (string.IsNullOrWhiteSpace(usuario.Id)) usuario.Id = Guid.NewGuid().ToString();
        _users.Add(usuario);
    }

    public void Update(UsuarioViewModel usuario)
    {
        var existing = _users.FirstOrDefault(u => u.Id == usuario.Id);
        if (existing is null) throw new InvalidOperationException("Usuário não encontrado");

    // Atualizar campos
        existing.Nome = usuario.Nome;
        existing.CPF = usuario.CPF;
        existing.RG = usuario.RG;
        existing.Email = usuario.Email;
        existing.Login = usuario.Login;
        existing.Telefone = usuario.Telefone;
        existing.IsActive = usuario.IsActive;

    // Se uma nova senha for fornecida, hashear e substituir
        if (!string.IsNullOrEmpty(usuario.Senha))
        {
            existing.Senha = _hasher.HashPassword(null, usuario.Senha);
        }
    // caso contrário, manter a senha hasheada existente
    }

    public bool ExistsByCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var norm = new string(cpf.Where(char.IsDigit).ToArray());
        return _users.Any(u => !string.IsNullOrWhiteSpace(u.CPF) && new string(u.CPF.Where(char.IsDigit).ToArray()) == norm);
    }

    public bool ChangePassword(string id, string senhaAtual, string novaSenha)
    {
        var existing = _users.FirstOrDefault(u => u.Id == id);
        if (existing is null) return false;

        // Verificar senha atual
        var verificationResult = _hasher.VerifyHashedPassword(null, existing.Senha, senhaAtual);
        
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return false; // Senha atual incorreta
        }

        // Atualizar para nova senha
        existing.Senha = _hasher.HashPassword(null, novaSenha);
        return true;
    }
}
