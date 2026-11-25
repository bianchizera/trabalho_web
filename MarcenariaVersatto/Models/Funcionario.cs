using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MarcenariaVersatto.Validation;

namespace MarcenariaVersatto.Models
{
    [Table("funcionarios")]
    public class Funcionario
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

        [Column("cpf")]
        [Display(Name = "CPF")]
        [StringLength(14, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        [Documento(TipoDocumento.Cpf)]
        public string Cpf { get; set; }

        [Column("cargo")]
        [Display(Name = "Cargo")]
        [Required(ErrorMessage = "Selecione o cargo.")]
        [StringLength(50, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        [RegularExpression("^(marceneiro|projetista|montador|administrativo|gerente)$", ErrorMessage = "Cargo inválido.")]
        public string Cargo { get; set; }

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

        [Column("data_admissao")]
        [Display(Name = "Data de admissão")]
        [DataType(DataType.Date)]
        public DateTime DataAdmissao { get; set; } = DateTime.Today;

        [Column("ativo")]
        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        public static readonly string[] Cargos = { "marceneiro", "projetista", "montador", "administrativo", "gerente" };
    }
}
