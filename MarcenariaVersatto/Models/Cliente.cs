using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MarcenariaVersatto.Validation;

namespace MarcenariaVersatto.Models
{
    [Table("clientes")]
    public class Cliente
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("nome")]
        [Display(Name = "Nome")]
        [Required(ErrorMessage = "Informe o nome.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre {2} e {1} caracteres.")]
        public string Nome { get; set; }

        [Column("tipo_pessoa")]
        [Display(Name = "Tipo de pessoa")]
        [Required]
        [StringLength(1, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        [RegularExpression("^[FJ]$", ErrorMessage = "Tipo de pessoa inválido.")]
        public string TipoPessoa { get; set; } = "F";

        [Column("cpf_cnpj")]
        [Display(Name = "CPF/CNPJ")]
        [StringLength(18, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        [Documento]
        public string CpfCnpj { get; set; }

        [Column("telefone")]
        [Display(Name = "Telefone")]
        [StringLength(20, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        [RegularExpression(@"^\(?\d{2}\)?[\s-]?9?\d{4}-?\d{4}$", ErrorMessage = "Telefone inválido. Ex.: (18) 99999-9999.")]
        public string Telefone { get; set; }

        [Column("email")]
        [Display(Name = "E-mail")]
        [StringLength(150, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; }

        [Column("endereco")]
        [Display(Name = "Endereço")]
        [StringLength(255, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        public string Endereco { get; set; }

        [Column("cidade")]
        [Display(Name = "Cidade")]
        [StringLength(100, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        public string Cidade { get; set; }

        [Column("criado_em")]
        [Display(Name = "Criado em")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CriadoEm { get; set; }

        public virtual ICollection<Orcamento> Orcamentos { get; set; } = new List<Orcamento>();
    }
}
