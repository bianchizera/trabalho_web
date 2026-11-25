using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarcenariaVersatto.Models
{
    [Table("orcamento_materiais")]
    public class OrcamentoMaterial
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("orcamento_id")]
        [Display(Name = "Orçamento")]
        [Required]
        public int OrcamentoId { get; set; }

        [ForeignKey("OrcamentoId")]
        public virtual Orcamento Orcamento { get; set; }

        [Column("material_id")]
        [Display(Name = "Material")]
        [Required(ErrorMessage = "Selecione o material.")]
        public int MaterialId { get; set; }

        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; }

        [Column("quantidade_estimada")]
        [Display(Name = "Quantidade estimada")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
        public decimal QuantidadeEstimada { get; set; }

        [Column("valor_unitario")]
        [Display(Name = "Valor unitário")]
        [Range(0, double.MaxValue, ErrorMessage = "{0} não pode ser negativo.")]
        public decimal ValorUnitario { get; set; }

        // Coluna GENERATED ALWAYS ... STORED no Postgres.
        [Column("valor_total")]
        [Display(Name = "Valor total")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal ValorTotal { get; set; }
    }
}
