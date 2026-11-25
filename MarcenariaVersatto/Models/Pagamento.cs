using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarcenariaVersatto.Models
{
    [Table("pagamentos")]
    public class Pagamento
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("orcamento_id")]
        [Display(Name = "Orçamento")]
        [Required(ErrorMessage = "Selecione o orçamento.")]
        public int OrcamentoId { get; set; }

        [ForeignKey("OrcamentoId")]
        public virtual Orcamento Orcamento { get; set; }

        [Column("valor")]
        [Display(Name = "Valor")]
        [Required(ErrorMessage = "Informe o valor.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Valor { get; set; }

        [Column("forma_pagamento")]
        [Display(Name = "Forma de pagamento")]
        [Required(ErrorMessage = "Selecione a forma de pagamento.")]
        [StringLength(20, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        [RegularExpression("^(pix|dinheiro|cartao|boleto|transferencia)$", ErrorMessage = "Forma de pagamento inválida.")]
        public string FormaPagamento { get; set; }

        [Column("data_pagamento")]
        [Display(Name = "Data do pagamento")]
        [DataType(DataType.Date)]
        public DateTime? DataPagamento { get; set; }

        [Column("status")]
        [Display(Name = "Status")]
        [Required]
        [StringLength(20, ErrorMessage = "{0} deve ter no máximo {1} caracteres.")]
        [RegularExpression("^(pendente|pago|atrasado)$", ErrorMessage = "Status inválido.")]
        public string Status { get; set; } = "pendente";

        public static readonly string[] FormasPagamento = { "pix", "dinheiro", "cartao", "boleto", "transferencia" };
        public static readonly string[] StatusOpcoes = { "pendente", "pago", "atrasado" };
    }
}
