using LogGet.Models;

namespace LogGet.Services;

public interface IMotoristaService
{
    IEnumerable<MotoristaViewModel> ListAll(string? query = null);
    void Add(MotoristaViewModel motorista);
    bool ExistsByCpf(string cpf);
}
