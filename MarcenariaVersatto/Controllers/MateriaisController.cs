using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarcenariaVersatto.Data;
using MarcenariaVersatto.Models;
using Microsoft.AspNetCore.Authorization;

namespace MarcenariaVersatto.Controllers
{
    [Authorize]
    public class MateriaisController : Controller
    {
        private readonly AppDbContext _context;

        public MateriaisController(AppDbContext context)
        {
            _context = context;
        }

        private async Task CarregarCombosAsync(int? categoriaId = null, int? fornecedorId = null)
        {
            ViewData["CategoriaId"] = new SelectList(await _context.CategoriasMaterial.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", categoriaId);
            ViewData["FornecedorPadraoId"] = new SelectList(await _context.Fornecedores.OrderBy(f => f.RazaoSocial).ToListAsync(), "Id", "RazaoSocial", fornecedorId);
        }

        public async Task<IActionResult> Index(string nome, int? categoriaId, string situacao)
        {
            var q = _context.Materiais.Include(m => m.Categoria).Include(m => m.FornecedorPadrao).AsQueryable();
            if (!string.IsNullOrWhiteSpace(nome))
            {
                var t = nome.Trim().ToLower();
                q = q.Where(m => m.Nome.ToLower().Contains(t));
            }
            if (categoriaId.HasValue) q = q.Where(m => m.CategoriaId == categoriaId.Value);
            if (situacao == "baixo") q = q.Where(m => m.EstoqueAtual < m.EstoqueMinimo);
            else if (situacao == "ok") q = q.Where(m => m.EstoqueAtual >= m.EstoqueMinimo);

            ViewData["f_nome"] = nome;
            ViewData["f_categoria"] = new SelectList(await _context.CategoriasMaterial.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", categoriaId);
            ViewData["f_situacao"] = new SelectList(new[] { new { V = "baixo", T = "Abaixo do mínimo" }, new { V = "ok", T = "Estoque OK" } }, "V", "T", situacao);
            return View(await q.OrderBy(m => m.Nome).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var material = await _context.Materiais
                .Include(m => m.Categoria).Include(m => m.FornecedorPadrao)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null) return NotFound();
            return View(material);
        }

        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Create()
        {
            await CarregarCombosAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Create([Bind("CategoriaId,FornecedorPadraoId,Nome,UnidadeMedida,EstoqueMinimo,EstoqueAtual,ValorUnitario")] Material material)
        {
            await ValidarMaterialAsync(material);
            if (ModelState.IsValid)
            {
                _context.Add(material);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await CarregarCombosAsync(material.CategoriaId, material.FornecedorPadraoId);
            return View(material);
        }

        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var material = await _context.Materiais.FindAsync(id);
            if (material == null) return NotFound();
            await CarregarCombosAsync(material.CategoriaId, material.FornecedorPadraoId);
            return View(material);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoriaId,FornecedorPadraoId,Nome,UnidadeMedida,EstoqueMinimo,EstoqueAtual,ValorUnitario")] Material material)
        {
            if (id != material.Id) return NotFound();

            await ValidarMaterialAsync(material);
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(material);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialExists(material.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await CarregarCombosAsync(material.CategoriaId, material.FornecedorPadraoId);
            return View(material);
        }

        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var material = await _context.Materiais
                .Include(m => m.Categoria).Include(m => m.FornecedorPadrao)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null) return NotFound();
            return View(material);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Perfis.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.Materiais.FindAsync(id);
            if (material != null)
            {
                try
                {
                    _context.Materiais.Remove(material);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Erro"] = "Não foi possível excluir: o material possui movimentações ou está em orçamentos.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MaterialExists(int id) => _context.Materiais.Any(e => e.Id == id);

        private async Task ValidarMaterialAsync(Material m)
        {
            if (!string.IsNullOrWhiteSpace(m.Nome))
            {
                var nome = m.Nome.Trim().ToLower();
                if (await _context.Materiais.AnyAsync(x => x.Nome.ToLower() == nome && x.Id != m.Id))
                    ModelState.AddModelError(nameof(m.Nome), "Este material já está cadastrado.");
            }
            if (m.CategoriaId > 0 && !await _context.CategoriasMaterial.AnyAsync(c => c.Id == m.CategoriaId))
                ModelState.AddModelError(nameof(m.CategoriaId), "Categoria não encontrada.");
        }
    }
}
