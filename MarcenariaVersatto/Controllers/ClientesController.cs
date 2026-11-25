using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarcenariaVersatto.Data;
using MarcenariaVersatto.Models;
using Microsoft.AspNetCore.Authorization;

namespace MarcenariaVersatto.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string nome, string cidade, string tipo)
        {
            var q = _context.Clientes.AsQueryable();
            if (!string.IsNullOrWhiteSpace(nome))
            {
                var t = nome.Trim().ToLower();
                q = q.Where(c => c.Nome.ToLower().Contains(t));
            }
            if (!string.IsNullOrWhiteSpace(cidade))
            {
                var t = cidade.Trim().ToLower();
                q = q.Where(c => c.Cidade != null && c.Cidade.ToLower().Contains(t));
            }
            if (!string.IsNullOrWhiteSpace(tipo)) q = q.Where(c => c.TipoPessoa == tipo);

            ViewData["f_nome"] = nome;
            ViewData["f_cidade"] = cidade;
            ViewData["f_tipo"] = new SelectList(new[] { new { V = "F", T = "Física" }, new { V = "J", T = "Jurídica" } }, "V", "T", tipo);
            return View(await q.OrderBy(c => c.Nome).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _context.Clientes.FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,TipoPessoa,CpfCnpj,Telefone,Email,Endereco,Cidade")] Cliente cliente)
        {
            await ValidarClienteAsync(cliente);
            if (ModelState.IsValid)
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,TipoPessoa,CpfCnpj,Telefone,Email,Endereco,Cidade")] Cliente cliente)
        {
            if (id != cliente.Id) return NotFound();

            await ValidarClienteAsync(cliente);
            if (ModelState.IsValid)
            {
                try
                {
                    // valor gerado pelo banco (criado_em) não deve ser sobrescrito
                    _context.Entry(cliente).Property(c => c.CriadoEm).IsModified = false;
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _context.Clientes.FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                try
                {
                    _context.Clientes.Remove(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Erro"] = "Não foi possível excluir: o cliente possui orçamentos vinculados.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id) => _context.Clientes.Any(e => e.Id == id);

        private async Task ValidarClienteAsync(Cliente c)
        {
            if (string.IsNullOrWhiteSpace(c.CpfCnpj)) return;

            var digitos = new string(c.CpfCnpj.Where(char.IsDigit).ToArray());
            if (c.TipoPessoa == "F" && digitos.Length != 11)
                ModelState.AddModelError(nameof(c.CpfCnpj), "Para pessoa física informe um CPF com 11 dígitos.");
            else if (c.TipoPessoa == "J" && digitos.Length != 14)
                ModelState.AddModelError(nameof(c.CpfCnpj), "Para pessoa jurídica informe um CNPJ com 14 dígitos.");

            if (await _context.Clientes.AnyAsync(x => x.CpfCnpj == c.CpfCnpj && x.Id != c.Id))
                ModelState.AddModelError(nameof(c.CpfCnpj), "Já existe um cliente cadastrado com este CPF/CNPJ.");
        }
    }
}
