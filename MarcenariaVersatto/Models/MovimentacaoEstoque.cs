using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarcenariaVersatto.Models
{
    [Table("movimentacoes_estoque")]
    public class MovimentacaoEstoque
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("material_id")]
        [Display(Name = "Material")]
        [Required(ErrorMessage = "Selecione o material.")]
        public int MaterialId { get; set; }

        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; }

        [Column("tipo")]
        [Display(Name = "Tipo")]
        [Required(ErrorMessage = "Selecione o tipo de movimentação.")]
        [StringLength(7, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        [RegularExpression("^(entrada|saida)$", ErrorMessage = "Tipo de movimentação inválido.")]
        public string Tipo { get; set; }

        [Column("quantidade")]
        [Display(Name = "Quantidade")]
        [Required(ErrorMessage = "Informe a quantidade.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
        public decimal Quantidade { get; set; }

        [Column("data_movimentacao")]
        [Display(Name = "Data")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DataMovimentacao { get; set; }

        [Column("motivo")]
        [Display(Name = "Motivo")]
        [StringLength(255, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        public string Motivo { get; set; }

        [Column("fornecedor_id")]
        [Display(Name = "Fornecedor")]
        public int? FornecedorId { get; set; }

        [ForeignKey("FornecedorId")]
        public virtual Fornecedor Fornecedor { get; set; }

        [Column("funcionario_id")]
        [Display(Name = "Funcionário responsável")]
        [Required(ErrorMessage = "Selecione o funcionário.")]
        public int FuncionarioId { get; set; }

        [ForeignKey("FuncionarioId")]
        public virtual Funcionario Funcionario { get; set; }

        public static readonly string[] Tipos = { "entrada", "saida" };
    }
}
