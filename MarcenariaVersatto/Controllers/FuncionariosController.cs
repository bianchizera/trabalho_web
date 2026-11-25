using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarcenariaVersatto.Data;
using MarcenariaVersatto.Models;
using Microsoft.AspNetCore.Authorization;

namespace MarcenariaVersatto.Controllers
{
    [Authorize(Roles = Perfis.Admin)]
    public class FuncionariosController : Controller
    {
        private readonly AppDbContext _context;

        public FuncionariosController(AppDbContext context)
        {
            _context = context;
        }

        private void CarregarCargos(string selecionado = null)
        {
            ViewData["Cargo"] = new SelectList(Funcionario.Cargos, selecionado);
        }

        public async Task<IActionResult> Index(string nome, string cargo, string ativo)
        {
            var q = _context.Funcionarios.AsQueryable();
            if (!string.IsNullOrWhiteSpace(nome))
            {
                var t = nome.Trim().ToLower();
                q = q.Where(f => f.Nome.ToLower().Contains(t));
            }
            if (!string.IsNullOrWhiteSpace(cargo)) q = q.Where(f => f.Cargo == cargo);
            if (ativo == "sim") q = q.Where(f => f.Ativo);
            else if (ativo == "nao") q = q.Where(f => !f.Ativo);

            ViewData["f_nome"] = nome;
            ViewData["f_cargo"] = new SelectList(Funcionario.Cargos, cargo);
            ViewData["f_ativo"] = new SelectList(new[] { new { V = "sim", T = "Sim" }, new { V = "nao", T = "Não" } }, "V", "T", ativo);
            return View(await q.OrderBy(f => f.Nome).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var funcionario = await _context.Funcionarios.FirstOrDefaultAsync(m => m.Id == id);
            if (funcionario == null) return NotFound();
            return View(funcionario);
        }

        public IActionResult Create()
        {
            CarregarCargos();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,Cpf,Cargo,Telefone,Email,DataAdmissao,Ativo")] Funcionario funcionario)
        {
            await ValidarFuncionarioAsync(funcionario);
            if (ModelState.IsValid)
            {
                _context.Add(funcionario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            CarregarCargos(funcionario.Cargo);
            return View(funcionario);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var funcionario = await _context.Funcionarios.FindAsync(id);
            if (funcionario == null) return NotFound();
            CarregarCargos(funcionario.Cargo);
            return View(funcionario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Cpf,Cargo,Telefone,Email,DataAdmissao,Ativo")] Funcionario funcionario)
        {
            if (id != funcionario.Id) return NotFound();

            await ValidarFuncionarioAsync(funcionario);
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(funcionario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FuncionarioExists(funcionario.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            CarregarCargos(funcionario.Cargo);
            return View(funcionario);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var funcionario = await _context.Funcionarios.FirstOrDefaultAsync(m => m.Id == id);
            if (funcionario == null) return NotFound();
            return View(funcionario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);
            if (funcionario != null)
            {
                try
                {
                    _context.Funcionarios.Remove(funcionario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Erro"] = "Não foi possível excluir: o funcionário possui movimentações ou usuário vinculados. Marque-o como inativo.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FuncionarioExists(int id) => _context.Funcionarios.Any(e => e.Id == id);

        private async Task ValidarFuncionarioAsync(Funcionario f)
        {
            if (!string.IsNullOrWhiteSpace(f.Cpf) &&
                await _context.Funcionarios.AnyAsync(x => x.Cpf == f.Cpf && x.Id != f.Id))
                ModelState.AddModelError(nameof(f.Cpf), "Já existe um funcionário cadastrado com este CPF.");

            if (f.DataAdmissao.Date > DateTime.Today)
                ModelState.AddModelError(nameof(f.DataAdmissao), "A data de admissão não pode ser futura.");
        }
    }
}
