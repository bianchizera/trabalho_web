using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MarcenariaVersatto.Validation;

namespace MarcenariaVersatto.Models
{
    [Table("fornecedores")]
    public class Fornecedor
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("razao_social")]
        [Display(Name = "Razão social")]
        [Required(ErrorMessage = "Informe a razão social.")]
        [StringLength(150, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        public string RazaoSocial { get; set; }

        [Column("cnpj")]
        [Display(Name = "CNPJ")]
        [StringLength(18, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        [Documento(TipoDocumento.Cnpj)]
        public string Cnpj { get; set; }

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

        public virtual ICollection<Material> Materiais { get; set; } = new List<Material>();
    }
}
