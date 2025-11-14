using LogGet.Models;

namespace LogGet.Services;

public interface IUsuarioService
{
    IEnumerable<UsuarioViewModel> ListAll(string? query = null);
    void Add(UsuarioViewModel usuario);
    void Update(UsuarioViewModel usuario);
    bool ExistsByCpf(string cpf);
    bool ChangePassword(string id, string senhaAtual, string novaSenha);
}
