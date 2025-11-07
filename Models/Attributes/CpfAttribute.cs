using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LogGet.Models.Attributes;

/// <summary>
/// Validação de CPF com suporte a validação do lado cliente via data-val-cpf.
/// </summary>
public class CpfAttribute : ValidationAttribute, IClientModelValidator
{
    public CpfAttribute() : base("CPF inválido.") { }

    public override bool IsValid(object? value)
    {
        var s = value as string;
        if (string.IsNullOrWhiteSpace(s)) return true; // [Required] cuida do obrigatório

        var digits = new string(s.Where(char.IsDigit).ToArray());
        if (digits.Length != 11) return false;

        // Rejeita CPFs com todos os dígitos iguais
        if (digits.Distinct().Count() == 1) return false;

        try
        {
            int[] nums = digits.Select(c => c - '0').ToArray();

            // Primeiro dígito verificador
            int sum = 0;
            for (int i = 0; i < 9; i++) sum += nums[i] * (10 - i);
            int rem = sum % 11;
            int dv1 = rem < 2 ? 0 : 11 - rem;
            if (nums[9] != dv1) return false;

            // Segundo dígito verificador
            sum = 0;
            for (int i = 0; i < 10; i++) sum += nums[i] * (11 - i);
            rem = sum % 11;
            int dv2 = rem < 2 ? 0 : 11 - rem;
            if (nums[10] != dv2) return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    // Implementa validação cliente via atributos data-val-*
    public void AddValidation(ClientModelValidationContext context)
    {
        if (context == null) return;
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-cpf", ErrorMessage ?? "CPF inválido.");
    }

    private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (attributes.ContainsKey(key)) return false;
        attributes.Add(key, value);
        return true;
    }
}
