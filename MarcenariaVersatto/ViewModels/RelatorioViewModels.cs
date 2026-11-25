namespace MarcenariaVersatto.ViewModels
{
    // Consulta com múltiplas tabelas + totalização: movimentacoes_estoque x materiais x categorias_material
    public class MovimentoMaterialLinha
    {
        public int MaterialId { get; set; }
        public string Material { get; set; }
        public string Categoria { get; set; }
        public string Unidade { get; set; }
        public decimal ValorUnitario { get; set; }
        public int NumMovimentacoes { get; set; }
        public decimal Entradas { get; set; }
        public decimal Saidas { get; set; }
        public decimal Saldo => Entradas - Saidas;
        public decimal ValorEntradas => Entradas * ValorUnitario;
        public decimal ValorSaidas => Saidas * ValorUnitario;
    }

    // Consulta com múltiplas tabelas + totalização: clientes x orcamentos x pagamentos
    public class OrcamentoClienteLinha
    {
        public int ClienteId { get; set; }
        public string Cliente { get; set; }
        public int NumOrcamentos { get; set; }
        public decimal TotalOrcado { get; set; }
        public decimal TotalAprovado { get; set; }
        public decimal TotalPago { get; set; }
        public decimal AReceber => TotalAprovado > TotalPago ? TotalAprovado - TotalPago : 0m;
    }

    // Agrupamento: materiais por categoria
    public class CategoriaResumoLinha
    {
        public string Categoria { get; set; }
        public int QtdMateriais { get; set; }
        public int AbaixoDoMinimo { get; set; }
        public decimal ValorEmEstoque { get; set; }
    }

    // Agrupamento: movimentações por mês
    public class MesResumoLinha
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
        public string Rotulo => $"{Mes:00}/{Ano}";
        public int NumEntradas { get; set; }
        public int NumSaidas { get; set; }
        public decimal ValorEntradas { get; set; }
        public decimal ValorSaidas { get; set; }
    }

    public class DadosDemoResumo
    {
        public int Clientes { get; set; }
        public int Fornecedores { get; set; }
        public int Materiais { get; set; }
        public int Movimentacoes { get; set; }
        public int Orcamentos { get; set; }
        public int Pagamentos { get; set; }
        public bool Existe => Clientes + Fornecedores + Materiais + Orcamentos > 0;
    }
}
