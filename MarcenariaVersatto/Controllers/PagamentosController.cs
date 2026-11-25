using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarcenariaVersatto.Data;
using MarcenariaVersatto.Models;
using Microsoft.AspNetCore.Authorization;

namespace MarcenariaVersatto.Controllers
{
    [Authorize(Roles = Perfis.Admin)]
    public class PagamentosController : Controller
    {
        private readonly AppDbContext _context;

        public PagamentosController(AppDbContext context)
        {
            _context = context;
        }

        private async Task CarregarCombosAsync(int? orcamentoId = null, string forma = null, string status = null)
        {
            ViewData["OrcamentoId"] = new SelectList(await _context.Orcamentos.Include(o => o.Cliente).OrderByDescending(o => o.Id).ToListAsync(), "Id", "Id", orcamentoId);
            ViewData["FormaPagamento"] = new SelectList(Pagamento.FormasPagamento, forma);
            ViewData["Status"] = new SelectList(Pagamento.StatusOpcoes, status);
        }

        public async Task<IActionResult> Index(string cliente, string status, string forma, DateTime? de, DateTime? ate)
        {
            var q = _context.Pagamentos.Include(p => p.Orcamento).ThenInclude(o => o.Cliente).AsQueryable();
            if (!string.IsNullOrWhiteSpace(cliente))
            {
                var t = cliente.Trim().ToLower();
                q = q.Where(p => p.Orcamento.Cliente.Nome.ToLower().Contains(t));
            }
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(p => p.Status == status);
            if (!string.IsNullOrWhiteSpace(forma)) q = q.Where(p => p.FormaPagamento == forma);
            if (de.HasValue)
            {
                var inicio = de.Value.Date;
                q = q.Where(p => p.DataPagamento >= inicio);
            }
            if (ate.HasValue)
            {
                var fim = ate.Value.Date;
                q = q.Where(p => p.DataPagamento <= fim);
            }

            ViewData["f_cliente"] = cliente;
            ViewData["f_status"] = new SelectList(Pagamento.StatusOpcoes, status);
            ViewData["f_forma"] = new SelectList(Pagamento.FormasPagamento, forma);
            ViewData["f_de"] = de?.ToString("yyyy-MM-dd");
            ViewData["f_ate"] = ate?.ToString("yyyy-MM-dd");
            return View(await q.OrderByDescending(p => p.Id).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var pagamento = await _context.Pagamentos
                .Include(p => p.Orcamento).ThenInclude(o => o.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pagamento == null) return NotFound();
            return View(pagamento);
        }

        public async Task<IActionResult> Create(int? orcamentoId)
        {
            await CarregarCombosAsync(orcamentoId);
            return View(new Pagamento { OrcamentoId = orcamentoId ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrcamentoId,Valor,FormaPagamento,DataPagamento,Status")] Pagamento pagamento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pagamento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await CarregarCombosAsync(pagamento.OrcamentoId, pagamento.FormaPagamento, pagamento.Status);
            return View(pagamento);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var pagamento = await _context.Pagamentos.FindAsync(id);
            if (pagamento == null) return NotFound();
            await CarregarCombosAsync(pagamento.OrcamentoId, pagamento.FormaPagamento, pagamento.Status);
            return View(pagamento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrcamentoId,Valor,FormaPagamento,DataPagamento,Status")] Pagamento pagamento)
        {
            if (id != pagamento.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pagamento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagamentoExists(pagamento.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await CarregarCombosAsync(pagamento.OrcamentoId, pagamento.FormaPagamento, pagamento.Status);
            return View(pagamento);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var pagamento = await _context.Pagamentos
                .Include(p => p.Orcamento).ThenInclude(o => o.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pagamento == null) return NotFound();
            return View(pagamento);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pagamento = await _context.Pagamentos.FindAsync(id);
            if (pagamento != null)
            {
                try
                {
                    _context.Pagamentos.Remove(pagamento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Erro"] = "Não foi possível excluir o pagamento.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PagamentoExists(int id) => _context.Pagamentos.Any(e => e.Id == id);
    }
}
