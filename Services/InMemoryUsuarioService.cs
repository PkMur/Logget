using LogGet.Models;

namespace LogGet.Services;

public class InMemoryUsuarioService : IUsuarioService
{
    private readonly List<UsuarioViewModel> _users = new();

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
        _users.Add(usuario);
    }

    public bool ExistsByCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var norm = new string(cpf.Where(char.IsDigit).ToArray());
        return _users.Any(u => !string.IsNullOrWhiteSpace(u.CPF) && new string(u.CPF.Where(char.IsDigit).ToArray()) == norm);
    }
}
