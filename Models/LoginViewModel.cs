using System.ComponentModel.DataAnnotations;

namespace LogGet.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o login.")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

