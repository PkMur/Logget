using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogGet.Models;

public class UsuarioViewModel
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

    public string Login { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}


