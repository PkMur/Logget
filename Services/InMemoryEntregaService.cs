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
    // Atribuir NumeroPedido sequencial se não fornecido: 0001, 0002, ... (apenas 4 dígitos)
        if (string.IsNullOrWhiteSpace(entrega.NumeroPedido))
        {
            var next = _entregas.Count + 1;
            entrega.NumeroPedido = $"{next:0000}";
        }

    // Garantir timestamp de criação
        entrega.CriadoEm = DateTime.UtcNow;

    // criar movimentação inicial
        entrega.Status = entrega.Status ?? "Criada";
        entrega.Movimentacoes.Add(new Movimentacao { Status = entrega.Status, Data = DateTime.UtcNow, Autor = "Sistema", Observacao = "Registro de entrega" });

        _entregas.Add(entrega);
    }

    public void Update(Entrega entrega)
    {
        var existing = _entregas.FirstOrDefault(e => e.Id == entrega.Id);
        if (existing is null)
            throw new InvalidOperationException("Entrega não encontrada.");

        // Atualizar apenas campos editáveis
        existing.DestinatarioNome = entrega.DestinatarioNome;
        existing.DestinatarioDocumento = entrega.DestinatarioDocumento;
        existing.EnderecoRua = entrega.EnderecoRua;
        existing.EnderecoNumero = entrega.EnderecoNumero;
        existing.EnderecoComplemento = entrega.EnderecoComplemento;
        existing.EnderecoBairro = entrega.EnderecoBairro;
        existing.EnderecoCidade = entrega.EnderecoCidade;
        existing.RemetenteNome = entrega.RemetenteNome;
        existing.QuantidadeVolumes = entrega.QuantidadeVolumes;
        existing.Peso = entrega.Peso;
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

    public bool AssignMotorista(string numeroPedido, string motoristaId, string motoristaNome, string usuarioAutor, out string? error)
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
    // definir status para 'Em rota' e adicionar movimentação
        entrega.Status = "Em rota";
        entrega.Movimentacoes.Insert(0, new Movimentacao { Status = entrega.Status, Data = DateTime.UtcNow, Autor = usuarioAutor, Observacao = $"Despachado para {motoristaNome}" });
        return true;
    }

    public Entrega? GetByNumeroPedido(string numeroPedido)
    {
        return _entregas.FirstOrDefault(e => string.Equals(e.NumeroPedido, numeroPedido, StringComparison.OrdinalIgnoreCase));
    }

    public bool MarcarComoEntregue(string numeroPedido, string usuarioAutor, out string? error)
    {
        error = null;
        var entrega = _entregas.FirstOrDefault(e => string.Equals(e.NumeroPedido, numeroPedido, StringComparison.OrdinalIgnoreCase));
        if (entrega is null)
        {
            error = $"Entrega {numeroPedido} não encontrada.";
            return false;
        }

        if (entrega.Status == "Entregue")
        {
            error = $"Entrega {numeroPedido} já foi finalizada.";
            return false;
        }

        // Atualizar status e adicionar movimentação
        entrega.Status = "Entregue";
        entrega.Movimentacoes.Insert(0, new Movimentacao 
        { 
            Status = "Entregue", 
            Data = DateTime.UtcNow, 
            Autor = usuarioAutor, 
            Observacao = "Entrega finalizada" 
        });
        
        return true;
    }
}


