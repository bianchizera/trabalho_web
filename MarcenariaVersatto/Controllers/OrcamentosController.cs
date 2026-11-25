using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarcenariaVersatto.Data;
using MarcenariaVersatto.Models;
using Microsoft.AspNetCore.Authorization;

namespace MarcenariaVersatto.Controllers
{
    [Authorize]
    public class OrcamentosController : Controller
    {
        private readonly AppDbContext _context;

        public OrcamentosController(AppDbContext context)
        {
            _context = context;
        }

        private async Task CarregarCombosAsync(int? clienteId = null, string status = null)
        {
            ViewData["ClienteId"] = new SelectList(await _context.Clientes.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", clienteId);
            ViewData["Status"] = new SelectList(Orcamento.StatusOpcoes, status);
        }

        public async Task<IActionResult> Index(int? clienteId, string status, DateTime? de, DateTime? ate)
        {
            var q = _context.Orcamentos.Include(o => o.Cliente).AsQueryable();
            if (clienteId.HasValue) q = q.Where(o => o.ClienteId == clienteId.Value);
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(o => o.Status == status);
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

            ViewData["f_cliente"] = new SelectList(await _context.Clientes.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", clienteId);
            ViewData["f_status"] = new SelectList(Orcamento.StatusOpcoes, status);
            ViewData["f_de"] = de?.ToString("yyyy-MM-dd");
            ViewData["f_ate"] = ate?.ToString("yyyy-MM-dd");
            return View(await q.OrderByDescending(o => o.DataOrcamento).ThenByDescending(o => o.Id).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var orcamento = await _context.Orcamentos
                .Include(o => o.Cliente)
                .Include(o => o.Materiais).ThenInclude(m => m.Material)
                .Include(o => o.Pagamentos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orcamento == null) return NotFound();
            return View(orcamento);
        }

        public async Task<IActionResult> Create()
        {
            await CarregarCombosAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClienteId,Versao,DataOrcamento,ValorMaoObra,Status")] Orcamento orcamento)
        {
            if (ModelState.IsValid)
            {
                orcamento.ValorMateriais = 0;
                _context.Add(orcamento);
                await _context.SaveChangesAsync();
                TempData["Info"] = "Orçamento criado. Agora adicione os materiais.";
                return RedirectToAction(nameof(Details), new { id = orcamento.Id });
            }
            await CarregarCombosAsync(orcamento.ClienteId, orcamento.Status);
            return View(orcamento);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var orcamento = await _context.Orcamentos.FindAsync(id);
            if (orcamento == null) return NotFound();
            await CarregarCombosAsync(orcamento.ClienteId, orcamento.Status);
            return View(orcamento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,Versao,DataOrcamento,ValorMaoObra,Status")] Orcamento orcamento)
        {
            if (id != orcamento.Id) return NotFound();

            var existente = await _context.Orcamentos.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
            if (existente == null) return NotFound();
            orcamento.ValorMateriais = existente.ValorMateriais;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orcamento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrcamentoExists(orcamento.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Details), new { id = orcamento.Id });
            }
            await CarregarCombosAsync(orcamento.ClienteId, orcamento.Status);
            return View(orcamento);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var orcamento = await _context.Orcamentos.Include(o => o.Cliente).FirstOrDefaultAsync(m => m.Id == id);
            if (orcamento == null) return NotFound();
            return View(orcamento);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orcamento = await _context.Orcamentos.FindAsync(id);
            if (orcamento != null)
            {
                try
                {
                    _context.Orcamentos.Remove(orcamento); // cascade apaga materiais/pagamentos do orçamento
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Erro"] = "Não foi possível excluir o orçamento.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // ---- Itens de material do orçamento (equivalente ao "item" de compra/venda no PHP) ----

        public async Task<IActionResult> AdicionarMaterial(int orcamentoId)
        {
            var orcamento = await _context.Orcamentos.FindAsync(orcamentoId);
            if (orcamento == null) return NotFound();
            ViewData["OrcamentoId"] = orcamentoId;
            ViewData["MaterialId"] = new SelectList(await _context.Materiais.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome");
            return View(new OrcamentoMaterial { OrcamentoId = orcamentoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarMaterial([Bind("OrcamentoId,MaterialId,QuantidadeEstimada,ValorUnitario")] OrcamentoMaterial item)
        {
            if (ModelState.IsValid)
            {
                _context.OrcamentoMateriais.Add(item);
                await RecalcularValorMateriaisAsync(item.OrcamentoId);
                return RedirectToAction(nameof(Details), new { id = item.OrcamentoId });
            }
            ViewData["OrcamentoId"] = item.OrcamentoId;
            ViewData["MaterialId"] = new SelectList(await _context.Materiais.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome", item.MaterialId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverMaterial(int id, int orcamentoId)
        {
            var item = await _context.OrcamentoMateriais.FindAsync(id);
            if (item != null)
            {
                _context.OrcamentoMateriais.Remove(item);
            }
            await RecalcularValorMateriaisAsync(orcamentoId);
            return RedirectToAction(nameof(Details), new { id = orcamentoId });
        }

        // Soma quantidade_estimada * valor_unitario de todos os itens e grava em orcamentos.valor_materiais
        // (valor_total é coluna GENERATED no banco e se recalcula sozinha a partir disso).
        private async Task RecalcularValorMateriaisAsync(int orcamentoId)
        {
            // grava a inclusão/remoção pendente do item antes de somar
            await _context.SaveChangesAsync();

            var soma = await _context.OrcamentoMateriais
                .Where(i => i.OrcamentoId == orcamentoId)
                .SumAsync(i => (decimal?)(i.QuantidadeEstimada * i.ValorUnitario)) ?? 0m;

            var orcamento = await _context.Orcamentos.FindAsync(orcamentoId);
            if (orcamento != null)
            {
                orcamento.ValorMateriais = soma;
                await _context.SaveChangesAsync();
            }
        }

        private bool OrcamentoExists(int id) => _context.Orcamentos.Any(e => e.Id == id);
    }
}
