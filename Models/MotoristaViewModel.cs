using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogGet.Models;

public class MotoristaViewModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    [Required(ErrorMessage = "Nome é obrigatório.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "CPF é obrigatório.")]
    [LogGet.Models.Attributes.Cpf]
    public string CPF { get; set; } = string.Empty;

    public string RG { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Tipo de habilitação")]
    [Required(ErrorMessage = "Tipo de habilitação é obrigatório.")]
    public string TipoHabilitacao { get; set; } = string.Empty;

    [Display(Name = "Número da CNH")]
    [Required(ErrorMessage = "Número da CNH é obrigatório.")]
    [RegularExpression(@"\d{11}", ErrorMessage = "CNH deve ter 11 dígitos.")]
    public string NumeroCnh { get; set; } = string.Empty;

    [Display(Name = "Veiculo")]
    [Required(ErrorMessage = "Veículo é obrigatório.")]
    public string Veiculo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Login é obrigatório.")]
    public string Login { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}


