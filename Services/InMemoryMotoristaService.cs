using LogGet.Models;

namespace LogGet.Services;

public class InMemoryMotoristaService : IMotoristaService
{
    private readonly List<MotoristaViewModel> _items = new();

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
        if (string.IsNullOrWhiteSpace(motorista.Id)) motorista.Id = Guid.NewGuid().ToString();
        _items.Add(motorista);
    }

    public bool ExistsByCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var norm = new string(cpf.Where(char.IsDigit).ToArray());
        return _items.Any(m => !string.IsNullOrWhiteSpace(m.CPF) && new string(m.CPF.Where(char.IsDigit).ToArray()) == norm);
    }
}
