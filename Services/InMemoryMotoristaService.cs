using LogGet.Models;
using Microsoft.AspNetCore.Identity;

namespace LogGet.Services;

public class InMemoryMotoristaService : IMotoristaService
{
    private readonly List<MotoristaViewModel> _items = new();
    private readonly PasswordHasher<object> _hasher = new();

    public IEnumerable<MotoristaViewModel> ListAll(string? query = null)
    {
        IEnumerable<MotoristaViewModel> data = _items.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.Trim();
            data = data.Where(m =>
                m.Nome.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                m.CPF.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                m.Veiculo.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        return data;
    }

    public void Add(MotoristaViewModel motorista)
    {
        // Garantir que a senha seja armazenada hasheada
        if (!string.IsNullOrEmpty(motorista.Senha))
        {
            motorista.Senha = _hasher.HashPassword(null, motorista.Senha);
        }

        if (string.IsNullOrWhiteSpace(motorista.Id)) motorista.Id = Guid.NewGuid().ToString();
        _items.Add(motorista);
    }

    public void Update(MotoristaViewModel motorista)
    {
        var existing = _items.FirstOrDefault(m => m.Id == motorista.Id);
        if (existing is null) throw new InvalidOperationException("Motorista não encontrado");

        existing.Nome = motorista.Nome;
        existing.CPF = motorista.CPF;
        existing.RG = motorista.RG;
        existing.Email = motorista.Email;
        existing.TipoHabilitacao = motorista.TipoHabilitacao;
        existing.NumeroCnh = motorista.NumeroCnh;
        existing.Veiculo = motorista.Veiculo;
        existing.Login = motorista.Login;
        existing.IsActive = motorista.IsActive;

        if (!string.IsNullOrEmpty(motorista.Senha))
        {
            existing.Senha = motorista.Senha;
        }
    }

    public bool ExistsByCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var norm = new string(cpf.Where(char.IsDigit).ToArray());
        return _items.Any(m => !string.IsNullOrWhiteSpace(m.CPF) && new string(m.CPF.Where(char.IsDigit).ToArray()) == norm);
    }

    public bool ChangePassword(string id, string senhaAtual, string novaSenha)
    {
        var existing = _items.FirstOrDefault(m => m.Id == id);
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
