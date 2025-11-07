using System.ComponentModel.DataAnnotations;

namespace LogGet.Models;

public class Movimentacao
{
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;

    [Display(Name = "Data")]
    public DateTime Data { get; set; } = DateTime.UtcNow;

    [Display(Name = "Quem")]
    public string Autor { get; set; } = string.Empty;

    [Display(Name = "Observação")]
    public string Observacao { get; set; } = string.Empty;
}
