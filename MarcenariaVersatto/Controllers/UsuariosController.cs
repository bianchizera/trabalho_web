using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarcenariaVersatto.Data;
using MarcenariaVersatto.Models;
using MarcenariaVersatto.ViewModels;

namespace MarcenariaVersatto.Controllers
{
    // UC04 - Gerenciar permissões de acesso (somente administrador)
    [Authorize(Roles = Perfis.Admin)]
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        private int? FuncionarioLogadoId() =>
            int.TryParse(User.FindFirst("FuncionarioId")?.Value, out var id) ? id : (int?)null;

        private static SelectList PerfisSelect(string selecionado = null) =>
            new SelectList(new[]
            {
                new { V = Perfis.Admin, T = "Administrador (acesso total)" },
                new { V = Perfis.Funcionario, T = "Funcionário (acesso básico)" }
            }, "V", "T", selecionado);

        private async Task CarregarFuncionariosLivresAsync(int? selecionado = null)
        {
            var comUsuario = await _context.Usuarios.Select(u => u.FuncionarioId).ToListAsync();
            var livres = await _context.Funcionarios
                .Where(f => f.Ativo && !comUsuario.Contains(f.Id))
                .OrderBy(f => f.Nome).ToListAsync();
            ViewData["FuncionarioId"] = new SelectList(livres, "Id", "Nome", selecionado);
        }

        public async Task<IActionResult> Index(string login, string perfil, string situacao)
        {
            var q = _context.Usuarios.Include(u => u.Funcionario).AsQueryable();
            if (!string.IsNullOrWhiteSpace(login))
            {
                var t = login.Trim().ToLower();
                q = q.Where(u => u.Login.ToLower().Contains(t) || u.Funcionario.Nome.ToLower().Contains(t));
            }
            if (!string.IsNullOrWhiteSpace(perfil)) q = q.Where(u => u.Perfil == perfil);
            if (situacao == "ativo") q = q.Where(u => u.Ativo);
            else if (situacao == "inativo") q = q.Where(u => !u.Ativo);

            ViewData["f_login"] = login;
            ViewData["f_perfil"] = PerfisSelect(perfil);
            ViewData["f_situacao"] = new SelectList(new[]
            {
                new { V = "ativo", T = "Ativos" },
                new { V = "inativo", T = "Inativos" }
            }, "V", "T", situacao);
            return View(await q.OrderBy(u => u.Login).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            await CarregarFuncionariosLivresAsync();
            ViewData["Perfil"] = PerfisSelect(Perfis.Funcionario);
            return View(new UsuarioCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioCreateViewModel model)
        {
            if (model.Login != null)
            {
                model.Login = model.Login.Trim();
                if (await _context.Usuarios.AnyAsync(u => u.Login == model.Login))
                    ModelState.AddModelError(nameof(model.Login), "Já existe um usuário com este login.");
            }

            var funcionario = await _context.Funcionarios.FindAsync(model.FuncionarioId);
            if (funcionario == null)
                ModelState.AddModelError(nameof(model.FuncionarioId), "Funcionário não encontrado.");
            else if (await _context.Usuarios.AnyAsync(u => u.FuncionarioId == model.FuncionarioId))
                ModelState.AddModelError(nameof(model.FuncionarioId), "Este funcionário já possui usuário.");

            if (ModelState.IsValid)
            {
                _context.Usuarios.Add(new Usuario
                {
                    FuncionarioId = model.FuncionarioId,
                    Login = model.Login,
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword(model.Senha),
                    Perfil = model.Perfil,
                    Ativo = true,
                    CriadoEm = DateTime.Now
                });
                await _context.SaveChangesAsync();
                TempData["Info"] = "Usuário cadastrado.";
                return RedirectToAction(nameof(Index));
            }

            await CarregarFuncionariosLivresAsync(model.FuncionarioId);
            ViewData["Perfil"] = PerfisSelect(model.Perfil);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var u = await _context.Usuarios.Include(x => x.Funcionario).FirstOrDefaultAsync(x => x.Id == id);
            if (u == null) return NotFound();

            ViewData["Perfil"] = PerfisSelect(u.Perfil);
            return View(new UsuarioEditViewModel
            {
                Id = u.Id,
                Login = u.Login,
                NomeFuncionario = u.Funcionario?.Nome,
                Perfil = u.Perfil,
                Ativo = u.Ativo
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioEditViewModel model)
        {
            if (id != model.Id) return NotFound();
            var u = await _context.Usuarios.Include(x => x.Funcionario).FirstOrDefaultAsync(x => x.Id == id);
            if (u == null) return NotFound();

            model.Login = u.Login;
            model.NomeFuncionario = u.Funcionario?.Nome;

            // Nunca deixar o sistema sem nenhum administrador ativo
            var perdeAdmin = u.Perfil == Perfis.Admin && u.Ativo && (model.Perfil != Perfis.Admin || !model.Ativo);
            if (perdeAdmin)
            {
                var outrosAdmins = await _context.Usuarios.CountAsync(x => x.Id != u.Id && x.Perfil == Perfis.Admin && x.Ativo);
                if (outrosAdmins == 0)
                    ModelState.AddModelError(string.Empty, "Este é o único administrador ativo. Cadastre outro administrador antes de alterar este.");
                else if (u.FuncionarioId == FuncionarioLogadoId())
                    ModelState.AddModelError(string.Empty, "Você não pode remover o seu próprio acesso de administrador.");
            }

            if (ModelState.IsValid)
            {
                u.Perfil = model.Perfil;
                u.Ativo = model.Ativo;
                if (!string.IsNullOrEmpty(model.NovaSenha))
                    u.SenhaHash = BCrypt.Net.BCrypt.HashPassword(model.NovaSenha);

                await _context.SaveChangesAsync();
                TempData["Info"] = "Usuário atualizado.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Perfil"] = PerfisSelect(model.Perfil);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var u = await _context.Usuarios.Include(x => x.Funcionario).FirstOrDefaultAsync(x => x.Id == id);
            if (u == null) return NotFound();
            return View(u);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var u = await _context.Usuarios.FindAsync(id);
            if (u == null) return RedirectToAction(nameof(Index));

            if (u.FuncionarioId == FuncionarioLogadoId())
            {
                TempData["Erro"] = "Você não pode excluir o seu próprio usuário.";
                return RedirectToAction(nameof(Index));
            }

            if (u.Perfil == Perfis.Admin && u.Ativo &&
                !await _context.Usuarios.AnyAsync(x => x.Id != u.Id && x.Perfil == Perfis.Admin && x.Ativo))
            {
                TempData["Erro"] = "Não é possível excluir o único administrador ativo.";
                return RedirectToAction(nameof(Index));
            }

            _context.Usuarios.Remove(u);
            await _context.SaveChangesAsync();
            TempData["Info"] = "Usuário excluído.";
            return RedirectToAction(nameof(Index));
        }
    }
}
