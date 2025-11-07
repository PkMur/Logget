using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogGet.Models;

public class Entrega
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [Display(Name = "Número do Pedido")]
    [Required]
    public string NumeroPedido { get; set; } = string.Empty;

    // Destinatário
    [Display(Name = "Destinatário")]
    public string DestinatarioNome { get; set; } = string.Empty;

    [Display(Name = "Documento do destinatário")]
    public string DestinatarioDocumento { get; set; } = string.Empty;

    // Endereço do destinatário (sub-bloco)
    [Display(Name = "Rua")]
    public string EnderecoRua { get; set; } = string.Empty;

    [Display(Name = "Número")]
    public string EnderecoNumero { get; set; } = string.Empty;

    [Display(Name = "Complemento")]
    public string EnderecoComplemento { get; set; } = string.Empty;

    [Display(Name = "Bairro")]
    public string EnderecoBairro { get; set; } = string.Empty;

    [Display(Name = "Cidade")]
    public string EnderecoCidade { get; set; } = string.Empty;

    // Remetente
    [Display(Name = "Remetente")]
    public string RemetenteNome { get; set; } = string.Empty;

    [Display(Name = "Quantidade de volumes")]
    public int QuantidadeVolumes { get; set; }

    [Display(Name = "Peso (kg)")]
    public decimal Peso { get; set; }

    // Data de criação registrada automaticamente
    [Display(Name = "Data de criação")]
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Motorista vinculado (se houver)
    [Display(Name = "Motorista Id")]
    public string MotoristaId { get; set; } = string.Empty;

    [Display(Name = "Motorista")]
    public string MotoristaNome { get; set; } = string.Empty;

    [Display(Name = "Status")]
    public string Status { get; set; } = "Criada";

    [Display(Name = "Movimentações")]
    public List<Movimentacao> Movimentacoes { get; set; } = new();

}


