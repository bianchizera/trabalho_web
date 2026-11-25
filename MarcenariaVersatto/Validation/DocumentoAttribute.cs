using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MarcenariaVersatto.Validation
{
    public enum TipoDocumento
    {
        CpfOuCnpj,
        Cpf,
        Cnpj
    }

    /// <summary>
    /// Valida CPF e/ou CNPJ (dígitos verificadores). Campo vazio é válido (use [Required] para obrigar).
    /// Também gera os atributos data-val-* para a validação no navegador (wwwroot/js/validacao.js).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DocumentoAttribute : ValidationAttribute, IClientModelValidator
    {
        public TipoDocumento Tipo { get; }

        public DocumentoAttribute(TipoDocumento tipo = TipoDocumento.CpfOuCnpj)
        {
            Tipo = tipo;
            ErrorMessage = tipo switch
            {
                TipoDocumento.Cpf => "CPF inválido.",
                TipoDocumento.Cnpj => "CNPJ inválido.",
                _ => "CPF/CNPJ inválido."
            };
        }

        public override bool IsValid(object value)
        {
            var texto = value as string;
            if (string.IsNullOrWhiteSpace(texto)) return true;

            var digitos = new string(texto.Where(char.IsDigit).ToArray());

            return Tipo switch
            {
                TipoDocumento.Cpf => ValidarCpf(digitos),
                TipoDocumento.Cnpj => ValidarCnpj(digitos),
                _ => digitos.Length == 11 ? ValidarCpf(digitos) : (digitos.Length == 14 && ValidarCnpj(digitos))
            };
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            Juntar(context.Attributes, "data-val", "true");
            Juntar(context.Attributes, "data-val-documento",
                FormatErrorMessage(context.ModelMetadata.DisplayName ?? context.ModelMetadata.Name));
            Juntar(context.Attributes, "data-val-documento-tipo", Tipo.ToString().ToLowerInvariant());
        }

        private static void Juntar(IDictionary<string, string> atributos, string chave, string valor)
        {
            if (!atributos.ContainsKey(chave)) atributos.Add(chave, valor);
        }

        public static bool ValidarCpf(string d)
        {
            if (d == null || d.Length != 11 || d.Distinct().Count() == 1) return false;

            int soma = 0;
            for (int i = 0; i < 9; i++) soma += (d[i] - '0') * (10 - i);
            int resto = soma * 10 % 11;
            if (resto == 10) resto = 0;
            if (resto != d[9] - '0') return false;

            soma = 0;
            for (int i = 0; i < 10; i++) soma += (d[i] - '0') * (11 - i);
            resto = soma * 10 % 11;
            if (resto == 10) resto = 0;
            return resto == d[10] - '0';
        }

        public static bool ValidarCnpj(string d)
        {
            if (d == null || d.Length != 14 || d.Distinct().Count() == 1) return false;

            int[] pesos1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] pesos2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            int soma = 0;
            for (int i = 0; i < 12; i++) soma += (d[i] - '0') * pesos1[i];
            int resto = soma % 11;
            int dv1 = resto < 2 ? 0 : 11 - resto;
            if (dv1 != d[12] - '0') return false;

            soma = 0;
            for (int i = 0; i < 13; i++) soma += (d[i] - '0') * pesos2[i];
            resto = soma % 11;
            int dv2 = resto < 2 ? 0 : 11 - resto;
            return dv2 == d[13] - '0';
        }
    }
}
