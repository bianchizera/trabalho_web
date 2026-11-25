using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarcenariaVersatto.Data;
using MarcenariaVersatto.Models;
using Microsoft.AspNetCore.Authorization;

namespace MarcenariaVersatto.Controllers
{
    [Authorize]
    public class FornecedoresController : Controller
    {
        private readonly AppDbContext _context;

        public FornecedoresController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string busca)
        {
            var q = _context.Fornecedores.AsQueryable();
            if (!string.IsNullOrWhiteSpace(busca))
            {
                var t = busca.Trim().ToLower();
                q = q.Where(f => f.RazaoSocial.ToLower().Contains(t)
                    || (f.Cnpj != null && f.Cnpj.Contains(t))
                    || (f.Email != null && f.Email.ToLower().Contains(t)));
            }
            ViewData["f_busca"] = busca;
            return View(await q.OrderBy(f => f.RazaoSocial).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var fornecedor = await _context.Fornecedores.FirstOrDefaultAsync(m => m.Id == id);
            if (fornecedor == null) return NotFound();
            return View(fornecedor);
        }

        [Authorize(Roles = Perfis.Admin)]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Create([Bind("RazaoSocial,Cnpj,Telefone,Email,Endereco")] Fornecedor fornecedor)
        {
            await ValidarFornecedorAsync(fornecedor);
            if (ModelState.IsValid)
            {
                _context.Add(fornecedor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fornecedor);
        }

        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var fornecedor = await _context.Fornecedores.FindAsync(id);
            if (fornecedor == null) return NotFound();
            return View(fornecedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RazaoSocial,Cnpj,Telefone,Email,Endereco")] Fornecedor fornecedor)
        {
            if (id != fornecedor.Id) return NotFound();

            await ValidarFornecedorAsync(fornecedor);
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fornecedor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FornecedorExists(fornecedor.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(fornecedor);
        }

        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var fornecedor = await _context.Fornecedores.FirstOrDefaultAsync(m => m.Id == id);
            if (fornecedor == null) return NotFound();
            return View(fornecedor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fornecedor = await _context.Fornecedores.FindAsync(id);
            if (fornecedor != null)
            {
                try
                {
                    _context.Fornecedores.Remove(fornecedor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Erro"] = "Não foi possível excluir: o fornecedor está vinculado a materiais ou movimentações.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FornecedorExists(int id) => _context.Fornecedores.Any(e => e.Id == id);

        private async Task ValidarFornecedorAsync(Fornecedor f)
        {
            if (!string.IsNullOrWhiteSpace(f.Cnpj) &&
                await _context.Fornecedores.AnyAsync(x => x.Cnpj == f.Cnpj && x.Id != f.Id))
                ModelState.AddModelError(nameof(f.Cnpj), "Já existe um fornecedor cadastrado com este CNPJ.");
        }
    }
}
