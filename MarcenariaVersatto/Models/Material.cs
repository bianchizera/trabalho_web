using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarcenariaVersatto.Models
{
    [Table("materiais")]
    public class Material
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("categoria_id")]
        [Display(Name = "Categoria")]
        [Required(ErrorMessage = "Selecione a categoria.")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual CategoriaMaterial Categoria { get; set; }

        [Column("fornecedor_padrao_id")]
        [Display(Name = "Fornecedor padrão")]
        public int? FornecedorPadraoId { get; set; }

        [ForeignKey("FornecedorPadraoId")]
        public virtual Fornecedor FornecedorPadrao { get; set; }

        [Column("nome")]
        [Display(Name = "Nome")]
        [Required(ErrorMessage = "Informe o nome do material.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve ter entre {2} e {1} caracteres.")]
        public string Nome { get; set; }

        [Column("unidade_medida")]
        [Display(Name = "Unidade de medida")]
        [Required(ErrorMessage = "Informe a unidade de medida.")]
        [StringLength(10, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        public string UnidadeMedida { get; set; }

        [Column("estoque_minimo")]
        [Display(Name = "Estoque mínimo")]
        [Range(0, double.MaxValue, ErrorMessage = "O estoque mínimo não pode ser negativo.")]
        public decimal EstoqueMinimo { get; set; }

        [Column("estoque_atual")]
        [Display(Name = "Estoque atual")]
        [Range(0, double.MaxValue, ErrorMessage = "O estoque atual não pode ser negativo.")]
        public decimal EstoqueAtual { get; set; }

        [Column("valor_unitario")]
        [Display(Name = "Valor unitário")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor não pode ser negativo.")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal ValorUnitario { get; set; }
    }
}
