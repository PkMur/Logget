using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogGet.Models;

public class MotoristaViewModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [LogGet.Models.Attributes.Cpf]
    public string CPF { get; set; } = string.Empty;

    [Required]
    public string RG { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Tipo de habilitação")]
    [Required]
    public string TipoHabilitacao { get; set; } = string.Empty;

    [Display(Name = "Número da CNH")]
    [Required]
    [RegularExpression(@"\d{11}", ErrorMessage = "CNH deve ter 11 dígitos.")]
    public string NumeroCnh { get; set; } = string.Empty;

    [Display(Name = "Veiculo")]
    [Required]
    public string Veiculo { get; set; } = string.Empty;

    [Required]
    public string Login { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;
}


