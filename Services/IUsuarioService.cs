using LogGet.Models;

namespace LogGet.Services;

public interface IUsuarioService
{
    IEnumerable<UsuarioViewModel> ListAll(string? query = null);
    void Add(UsuarioViewModel usuario);
    bool ExistsByCpf(string cpf);
}
