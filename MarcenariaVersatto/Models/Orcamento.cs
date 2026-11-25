using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarcenariaVersatto.Models
{
    [Table("orcamentos")]
    public class Orcamento
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("cliente_id")]
        [Display(Name = "Cliente")]
        [Required(ErrorMessage = "Selecione o cliente.")]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        [Column("versao")]
        [Display(Name = "Versão")]
        [Range(1, int.MaxValue, ErrorMessage = "A versão deve ser no mínimo 1.")]
        public int Versao { get; set; } = 1;

        [Column("data_orcamento")]
        [Display(Name = "Data do orçamento")]
        [DataType(DataType.Date)]
        public DateTime DataOrcamento { get; set; } = DateTime.Today;

        [Column("valor_materiais")]
        [Display(Name = "Valor materiais")]
        [Range(0, double.MaxValue, ErrorMessage = "{0} não pode ser negativo.")]
        public decimal ValorMateriais { get; set; }

        [Column("valor_mao_obra")]
        [Display(Name = "Valor mão de obra")]
        [Range(0, double.MaxValue, ErrorMessage = "{0} não pode ser negativo.")]
        public decimal ValorMaoObra { get; set; }

        // Coluna GENERATED ALWAYS ... STORED no Postgres: nunca é enviada no INSERT/UPDATE.
        [Column("valor_total")]
        [Display(Name = "Valor total")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal ValorTotal { get; set; }

        [Column("status")]
        [Display(Name = "Status")]
        [Required]
        [StringLength(20, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        [RegularExpression("^(pendente|aprovado|recusado)$", ErrorMessage = "Status inválido.")]
        public string Status { get; set; } = "pendente";

        public virtual ICollection<OrcamentoMaterial> Materiais { get; set; } = new List<OrcamentoMaterial>();
        public virtual ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();

        public static readonly string[] StatusOpcoes = { "pendente", "aprovado", "recusado" };
    }
}
