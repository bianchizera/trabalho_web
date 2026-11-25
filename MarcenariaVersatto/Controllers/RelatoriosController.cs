using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarcenariaVersatto.Data;
using MarcenariaVersatto.Models;
using MarcenariaVersatto.ViewModels;

namespace MarcenariaVersatto.Controllers
{
    [Authorize]
    public class RelatoriosController : Controller
    {
        private readonly AppDbContext _context;

        public RelatoriosController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        // UC10 - Emitir alertas de estoque baixo (somente administrador)
        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Alertas()
        {
            var criticos = await _context.Materiais
                .Include(m => m.Categoria)
                .Include(m => m.FornecedorPadrao)
                .Where(m => m.EstoqueAtual < m.EstoqueMinimo)
                .OrderBy(m => m.EstoqueAtual - m.EstoqueMinimo)
                .ToListAsync();
            return View(criticos);
        }

        // CONSULTA COM MÚLTIPLAS TABELAS + TOTALIZAÇÃO
        // movimentacoes_estoque x materiais x categorias_material: entradas, saídas, saldo e valores por material.
        public async Task<IActionResult> MovimentoPorMaterial(DateTime? de, DateTime? ate, int? categoriaId)
        {
            var q = _context.MovimentacoesEstoque.AsQueryable();
            if (de.HasValue)
            {
                var inicio = de.Value.Date;
                q = q.Where(m => m.DataMovimentacao >= inicio);
            }
            if (ate.HasValue)
            {
                var fim = ate.Value.Date.AddDays(1);
                q = q.Where(m => m.DataMovimentacao < fim);
            }
            if (categoriaId.HasValue) q = q.Where(m => m.Material.CategoriaId == categoriaId.Value);

            var linhas = await q.Select(m => new
            {
                m.MaterialId,
                Material = m.Material.Nome,
                Categoria = m.Material.Categoria.Nome,
                Unidade = m.Material.UnidadeMedida,
                ValorUnitario = m.Material.ValorUnitario,
                m.Tipo,
                m.Quantidade
            }).ToListAsync();

            var resumo = linhas
                .GroupBy(l => new { l.MaterialId, l.Material, l.Categoria, l.Unidade, l.ValorUnitario })
                .Select(g => new MovimentoMaterialLinha
                {
                    MaterialId = g.Key.MaterialId,
                    Material = g.Key.Material,
                    Categoria = g.Key.Categoria,
                    Unidade = g.Key.Unidade,
                    ValorUnitario = g.Key.ValorUnitario,
                    NumMovimentacoes = g.Count(),
                    Entradas = g.Where(x => x.Tipo == "entrada").Sum(x => x.Quantidade),
                    Saidas = g.Where(x => x.Tipo == "saida").Sum(x => x.Quantidade)
                })
                .OrderBy(r => r.Categoria).ThenBy(r => r.Material)
                .ToList();

            ViewData["f_de"] = de?.ToString("yyyy-MM-dd");
            ViewData["f_ate"] = ate?.ToString("yyyy-MM-dd");
            ViewData["f_categoria"] = new SelectList(await _context.CategoriasMaterial.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", categoriaId);
            return View(resumo);
        }

        // CONSULTA COM MÚLTIPLAS TABELAS + TOTALIZAÇÃO
        // clientes x orcamentos x pagamentos: quanto foi orçado, aprovado, pago e quanto falta receber por cliente.
        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> OrcamentosPorCliente(DateTime? de, DateTime? ate, string status)
        {
            var q = _context.Orcamentos.AsQueryable();
            if (de.HasValue)
            {
                var inicio = de.Value.Date;
                q = q.Where(o => o.DataOrcamento >= inicio);
            }
            if (ate.HasValue)
            {
                var fim = ate.Value.Date;
                q = q.Where(o => o.DataOrcamento <= fim);
            }
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(o => o.Status == status);

            var linhas = await q.Select(o => new
            {
                o.ClienteId,
                Cliente = o.Cliente.Nome,
                o.Status,
                o.ValorTotal,
                Pago = o.Pagamentos.Where(p => p.Status == "pago").Sum(p => (decimal?)p.Valor) ?? 0m
            }).ToListAsync();

            var resumo = linhas
                .GroupBy(l => new { l.ClienteId, l.Cliente })
                .Select(g => new OrcamentoClienteLinha
                {
                    ClienteId = g.Key.ClienteId,
                    Cliente = g.Key.Cliente,
                    NumOrcamentos = g.Count(),
                    TotalOrcado = g.Sum(x => x.ValorTotal),
                    TotalAprovado = g.Where(x => x.Status == "aprovado").Sum(x => x.ValorTotal),
                    TotalPago = g.Sum(x => x.Pago)
                })
                .OrderByDescending(r => r.TotalOrcado)
                .ToList();

            ViewData["f_de"] = de?.ToString("yyyy-MM-dd");
            ViewData["f_ate"] = ate?.ToString("yyyy-MM-dd");
            ViewData["f_status"] = new SelectList(Orcamento.StatusOpcoes, status);
            return View(resumo);
        }

        // CONSULTA DE AGRUPAMENTO: materiais agrupados por categoria
        public async Task<IActionResult> MateriaisPorCategoria()
        {
            var materiais = await _context.Materiais
                .Select(m => new
                {
                    Categoria = m.Categoria.Nome,
                    m.EstoqueAtual,
                    m.EstoqueMinimo,
                    m.ValorUnitario
                }).ToListAsync();

            var resumo = materiais
                .GroupBy(m => m.Categoria)
                .Select(g => new CategoriaResumoLinha
                {
                    Categoria = g.Key,
                    QtdMateriais = g.Count(),
                    AbaixoDoMinimo = g.Count(x => x.EstoqueAtual < x.EstoqueMinimo),
                    ValorEmEstoque = g.Sum(x => x.EstoqueAtual * x.ValorUnitario)
                })
                .OrderBy(r => r.Categoria)
                .ToList();

            return View(resumo);
        }

        // CONSULTA DE AGRUPAMENTO: movimentações agrupadas por mês (entradas x saídas)
        public async Task<IActionResult> MovimentacoesPorMes(int? ano)
        {
            var q = _context.MovimentacoesEstoque.AsQueryable();
            if (ano.HasValue)
            {
                var inicio = new DateTime(ano.Value, 1, 1);
                var fim = inicio.AddYears(1);
                q = q.Where(m => m.DataMovimentacao >= inicio && m.DataMovimentacao < fim);
            }

            var linhas = await q.Select(m => new
            {
                m.DataMovimentacao,
                m.Tipo,
                m.Quantidade,
                ValorUnitario = m.Material.ValorUnitario
            }).ToListAsync();

            var resumo = linhas
                .GroupBy(l => new { l.DataMovimentacao.Year, l.DataMovimentacao.Month })
                .Select(g => new MesResumoLinha
                {
                    Ano = g.Key.Year,
                    Mes = g.Key.Month,
                    NumEntradas = g.Count(x => x.Tipo == "entrada"),
                    NumSaidas = g.Count(x => x.Tipo == "saida"),
                    ValorEntradas = g.Where(x => x.Tipo == "entrada").Sum(x => x.Quantidade * x.ValorUnitario),
                    ValorSaidas = g.Where(x => x.Tipo == "saida").Sum(x => x.Quantidade * x.ValorUnitario)
                })
                .OrderByDescending(r => r.Ano).ThenByDescending(r => r.Mes)
                .ToList();

            var anos = await _context.MovimentacoesEstoque
                .Select(m => m.DataMovimentacao.Year).Distinct().ToListAsync();
            ViewData["f_ano"] = new SelectList(anos.OrderByDescending(a => a).Select(a => new { V = a, T = a.ToString() }), "V", "T", ano);
            return View(resumo);
        }
    }
}
