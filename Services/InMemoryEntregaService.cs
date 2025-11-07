using LogGet.Models;

namespace LogGet.Services;

public class InMemoryEntregaService : IEntregaService
{
    private readonly List<Entrega> _entregas = new();

    public IEnumerable<Entrega> ListAll(string? query = null)
    {
        IEnumerable<Entrega> data = _entregas.OrderByDescending(e => e.CriadoEm);
        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.Trim();
            data = data.Where(e =>
                e.NumeroPedido.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.DestinatarioNome.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.RemetenteNome.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.EnderecoRua.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.EnderecoCidade.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        return data;
    }

    public void Add(Entrega entrega)
    {
        // Assign sequential NumeroPedido if not provided: ENT0001, ENT0002, ...
        if (string.IsNullOrWhiteSpace(entrega.NumeroPedido))
        {
            var next = _entregas.Count + 1;
            entrega.NumeroPedido = $"ENT{next:0000}";
        }

        // Ensure created timestamp
        entrega.CriadoEm = DateTime.UtcNow;

        // create initial movement
        entrega.Status = entrega.Status ?? "Criada";
        entrega.Movimentacoes.Add(new Movimentacao { Status = entrega.Status, Data = DateTime.UtcNow, Autor = "Sistema", Observacao = "Registro de entrega" });

        _entregas.Add(entrega);
    }

    public IEnumerable<Entrega> ListWithoutMotorista(string? query = null)
    {
        IEnumerable<Entrega> data = _entregas.Where(e => string.IsNullOrWhiteSpace(e.MotoristaId)).OrderByDescending(e => e.CriadoEm);
        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.Trim();
            data = data.Where(e => e.NumeroPedido.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                   e.DestinatarioNome.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                   e.RemetenteNome.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        return data;
    }

    public bool AssignMotorista(string numeroPedido, string motoristaId, string motoristaNome, out string? error)
    {
        error = null;
        var entrega = _entregas.FirstOrDefault(e => string.Equals(e.NumeroPedido, numeroPedido, StringComparison.OrdinalIgnoreCase));
        if (entrega is null)
        {
            error = $"Entrega {numeroPedido} não encontrada.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(entrega.MotoristaId))
        {
            error = $"Entrega {numeroPedido} já possui motorista vinculado.";
            return false;
        }

        entrega.MotoristaId = motoristaId;
        entrega.MotoristaNome = motoristaNome;
        // set status to Em rota and add movement
        entrega.Status = "Em rota";
        entrega.Movimentacoes.Insert(0, new Movimentacao { Status = entrega.Status, Data = DateTime.UtcNow, Autor = motoristaNome, Observacao = "Despachado" });
        return true;
    }

    public Entrega? GetByNumeroPedido(string numeroPedido)
    {
        return _entregas.FirstOrDefault(e => string.Equals(e.NumeroPedido, numeroPedido, StringComparison.OrdinalIgnoreCase));
    }
}


