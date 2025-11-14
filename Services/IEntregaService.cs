using LogGet.Models;

namespace LogGet.Services;

public interface IEntregaService
{
    IEnumerable<Entrega> ListAll(string? query = null);
    void Add(Entrega entrega);
    void Update(Entrega entrega);
    IEnumerable<Entrega> ListWithoutMotorista(string? query = null);
    bool AssignMotorista(string numeroPedido, string motoristaId, string motoristaNome, string usuarioAutor, out string? error);
    Entrega? GetByNumeroPedido(string numeroPedido);
    bool MarcarComoEntregue(string numeroPedido, string usuarioAutor, out string? error);
}


