using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarcenariaVersatto.Data;
using MarcenariaVersatto.Models;
using Microsoft.AspNetCore.Authorization;

namespace MarcenariaVersatto.Controllers
{
    [Authorize]
    public class MovimentacoesEstoqueController : Controller
    {
        private readonly AppDbContext _context;

        public MovimentacoesEstoqueController(AppDbContext context)
        {
            _context = context;
        }

        private async Task CarregarCombosAsync(int? materialId = null, int? fornecedorId = null, int? funcionarioId = null, string tipo = null)
        {
            ViewData["MaterialId"] = new SelectList(await _context.Materiais.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome", materialId);
            ViewData["FornecedorId"] = new SelectList(await _context.Fornecedores.OrderBy(f => f.RazaoSocial).ToListAsync(), "Id", "RazaoSocial", fornecedorId);
            ViewData["FuncionarioId"] = new SelectList(await _context.Funcionarios.Where(f => f.Ativo).OrderBy(f => f.Nome).ToListAsync(), "Id", "Nome", funcionarioId);
            ViewData["Tipo"] = new SelectList(new[] { new { V = "entrada", T = "Entrada" }, new { V = "saida", T = "Saída" } }, "V", "T", tipo);
        }

        public async Task<IActionResult> Index(int? materialId, string tipo, int? funcionarioId, DateTime? de, DateTime? ate)
        {
            var q = _context.MovimentacoesEstoque
                .Include(m => m.Material).Include(m => m.Fornecedor).Include(m => m.Funcionario)
                .AsQueryable();
            if (materialId.HasValue) q = q.Where(m => m.MaterialId == materialId.Value);
            if (!string.IsNullOrWhiteSpace(tipo)) q = q.Where(m => m.Tipo == tipo);
            if (funcionarioId.HasValue) q = q.Where(m => m.FuncionarioId == funcionarioId.Value);
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

            ViewData["f_material"] = new SelectList(await _context.Materiais.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome", materialId);
            ViewData["f_tipo"] = new SelectList(new[] { new { V = "entrada", T = "Entrada" }, new { V = "saida", T = "Saída" } }, "V", "T", tipo);
            ViewData["f_funcionario"] = new SelectList(await _context.Funcionarios.OrderBy(f => f.Nome).ToListAsync(), "Id", "Nome", funcionarioId);
            ViewData["f_de"] = de?.ToString("yyyy-MM-dd");
            ViewData["f_ate"] = ate?.ToString("yyyy-MM-dd");
            return View(await q.OrderByDescending(m => m.DataMovimentacao).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var mov = await _context.MovimentacoesEstoque
                .Include(m => m.Material).Include(m => m.Fornecedor).Include(m => m.Funcionario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mov == null) return NotFound();
            return View(mov);
        }

        private int? FuncionarioLogadoId() =>
            int.TryParse(User.FindFirst("FuncionarioId")?.Value, out var id) ? id : (int?)null;

        // UC03 / UC07: "Registrar Entrada" e "Registrar Saída" abrem este mesmo formulário com o tipo já escolhido.
        public async Task<IActionResult> Create(string tipo)
        {
            var mov = new MovimentacaoEstoque
            {
                Tipo = tipo == "saida" ? "saida" : "entrada",
                FuncionarioId = FuncionarioLogadoId() ?? 0
            };
            await CarregarCombosAsync(null, null, FuncionarioLogadoId(), mov.Tipo);
            await CarregarNomeResponsavelAsync(mov.FuncionarioId);
            return View(mov);
        }

        private async Task CarregarNomeResponsavelAsync(int funcionarioId)
        {
            var f = await _context.Funcionarios.FindAsync(funcionarioId);
            ViewData["NomeFuncionario"] = f?.Nome;
        }

        // A tabela movimentacoes_estoque tem um trigger no Postgres que já soma/subtrai
        // automaticamente de materiais.estoque_atual a cada INSERT — não fazemos isso aqui.
        // Aqui validamos (antes de gravar): material existe, quantidade válida e, na saída, estoque suficiente.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaterialId,Tipo,Quantidade,Motivo,FornecedorId,FuncionarioId")] MovimentacaoEstoque movimentacao)
        {
            // Quem não é administrador sempre registra em seu próprio nome
            var logado = FuncionarioLogadoId();
            if (!User.IsInRole(Perfis.Admin) && logado.HasValue)
            {
                movimentacao.FuncionarioId = logado.Value;
                ModelState.Remove(nameof(MovimentacaoEstoque.FuncionarioId));
            }

            var material = await _context.Materiais.FindAsync(movimentacao.MaterialId);
            if (material == null)
            {
                ModelState.AddModelError(nameof(movimentacao.MaterialId), "Material não encontrado.");
            }
            else if (movimentacao.Tipo == "saida" && material.EstoqueAtual < movimentacao.Quantidade)
            {
                ModelState.AddModelError(string.Empty, $"Estoque insuficiente para \"{material.Nome}\". Disponível: {material.EstoqueAtual} {material.UnidadeMedida}.");
            }

            if (movimentacao.Tipo == "saida") movimentacao.FornecedorId = null; // fornecedor só faz sentido em entrada

            if (ModelState.IsValid)
            {
                _context.Add(movimentacao);
                await _context.SaveChangesAsync();
                TempData["Info"] = movimentacao.Tipo == "entrada" ? "Entrada de estoque registrada." : "Saída de estoque registrada.";
                return RedirectToAction(nameof(Index));
            }
            await CarregarCombosAsync(movimentacao.MaterialId, movimentacao.FornecedorId, movimentacao.FuncionarioId, movimentacao.Tipo);
            await CarregarNomeResponsavelAsync(movimentacao.FuncionarioId);
            return View(movimentacao);
        }

        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var mov = await _context.MovimentacoesEstoque
                .Include(m => m.Material).Include(m => m.Fornecedor).Include(m => m.Funcionario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mov == null) return NotFound();
            return View(mov);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Atenção: apagar uma movimentação NÃO desfaz o ajuste de estoque feito pelo trigger na inserção.
            var mov = await _context.MovimentacoesEstoque.FindAsync(id);
            if (mov != null)
            {
                try
                {
                    _context.MovimentacoesEstoque.Remove(mov);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Erro"] = "Não foi possível excluir a movimentação.";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
