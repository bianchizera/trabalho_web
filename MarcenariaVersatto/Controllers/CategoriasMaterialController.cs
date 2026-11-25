using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarcenariaVersatto.Data;
using MarcenariaVersatto.Models;
using Microsoft.AspNetCore.Authorization;

namespace MarcenariaVersatto.Controllers
{
    [Authorize]
    public class CategoriasMaterialController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriasMaterialController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string nome)
        {
            var q = _context.CategoriasMaterial.AsQueryable();
            if (!string.IsNullOrWhiteSpace(nome))
            {
                var t = nome.Trim().ToLower();
                q = q.Where(c => c.Nome.ToLower().Contains(t));
            }
            ViewData["f_nome"] = nome;
            return View(await q.OrderBy(c => c.Nome).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _context.CategoriasMaterial.FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        [Authorize(Roles = Perfis.Admin)]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Create([Bind("Nome")] CategoriaMaterial categoria)
        {
            await ValidarCategoriaAsync(categoria);
            if (ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _context.CategoriasMaterial.FindAsync(id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] CategoriaMaterial categoria)
        {
            if (id != categoria.Id) return NotFound();

            await ValidarCategoriaAsync(categoria);
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _context.CategoriasMaterial.FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.CategoriasMaterial.FindAsync(id);
            if (categoria != null)
            {
                try
                {
                    _context.CategoriasMaterial.Remove(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Erro"] = "Não foi possível excluir: existem materiais nesta categoria.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int id) => _context.CategoriasMaterial.Any(e => e.Id == id);

        private async Task ValidarCategoriaAsync(CategoriaMaterial c)
        {
            if (string.IsNullOrWhiteSpace(c.Nome)) return;
            var nome = c.Nome.Trim().ToLower();
            if (await _context.CategoriasMaterial.AnyAsync(x => x.Nome.ToLower() == nome && x.Id != c.Id))
                ModelState.AddModelError(nameof(c.Nome), "Já existe uma categoria com este nome.");
        }
    }
}
