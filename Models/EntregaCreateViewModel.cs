using System.ComponentModel.DataAnnotations;

namespace LogGet.Models;

public class EntregaCreateViewModel
{
    // Numero do pedido será gerado automaticamente pelo sistema

    // Destinatário
    [Display(Name = "Nome do destinatário")]
    [Required]
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
    [Display(Name = "Nome do remetente")]
    public string RemetenteNome { get; set; } = string.Empty;

    [Display(Name = "Quantidade de volumes")]
    public int QuantidadeVolumes { get; set; }

    [Display(Name = "Peso (kg)")]
    public decimal Peso { get; set; }

    [Display(Name = "Remetente")]
    public string Remetente { get; set; } = string.Empty;

    [Display(Name = "Status")]
    public string Status { get; set; } = "Pendente";

    [Display(Name = "Data de Início")]
    public DateTime? DataInicio { get; set; }
}


