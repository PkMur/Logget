using LogGet.Models;

namespace LogGet.Services;

public interface IMotoristaService
{
    IEnumerable<MotoristaViewModel> ListAll(string? query = null);
    void Add(MotoristaViewModel motorista);
    void Update(MotoristaViewModel motorista);
    bool ExistsByCpf(string cpf);
    bool ChangePassword(string id, string senhaAtual, string novaSenha);
}
