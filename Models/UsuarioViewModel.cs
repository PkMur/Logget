using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogGet.Models;

public class UsuarioViewModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [LogGet.Models.Attributes.Cpf]
    public string CPF { get; set; } = string.Empty;

    public string RG { get; set; } = string.Empty;

    [Display(Name = "Nome da mãe")]
    [Required]
    public string NomeDaMae { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Login { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;
}


