using LogGet.Models;

namespace LogGet.Services;

public interface IEntregaService
{
    IEnumerable<Entrega> ListAll(string? query = null);
    void Add(Entrega entrega);
    IEnumerable<Entrega> ListWithoutMotorista(string? query = null);
    bool AssignMotorista(string numeroPedido, string motoristaId, string motoristaNome, out string? error);
    Entrega? GetByNumeroPedido(string numeroPedido);
}


