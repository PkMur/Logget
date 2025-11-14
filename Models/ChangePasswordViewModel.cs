using System.ComponentModel.DataAnnotations;

namespace LogGet.Models;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Senha atual é obrigatória.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha atual")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova senha")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar nova senha")]
    [Compare("NovaSenha", ErrorMessage = "As senhas não coincidem.")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}
