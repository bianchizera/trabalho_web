using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MarcenariaVersatto.Data;
using MarcenariaVersatto.Models;
using MarcenariaVersatto.ViewModels;

namespace MarcenariaVersatto.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            ViewBag.SemUsuarios = !await _context.Usuarios.AnyAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string login, string senha)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Funcionario)
                .FirstOrDefaultAsync(u => u.Login == login && u.Ativo);

            if (usuario == null || string.IsNullOrEmpty(senha) || !BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash))
            {
                ModelState.AddModelError(string.Empty, "Login ou senha inválidos.");
                ViewBag.SemUsuarios = !await _context.Usuarios.AnyAsync();
                return View();
            }

            // Perfil desconhecido nunca vira admin
            var perfil = usuario.Perfil == Perfis.Admin ? Perfis.Admin : Perfis.Funcionario;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Login),
                new Claim(ClaimTypes.Role, perfil),
                new Claim("FuncionarioId", usuario.FuncionarioId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            usuario.UltimoAcesso = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        // Só funciona enquanto NÃO existe nenhum usuário no banco: cria o primeiro administrador.
        // (Substitui o antigo endpoint CriarUsuarioTeste, que deixava um admin/123456 aberto para qualquer pessoa.)
        [HttpGet]
        public async Task<IActionResult> PrimeiroAcesso()
        {
            if (await _context.Usuarios.AnyAsync()) return RedirectToAction(nameof(Login));
            return View(new PrimeiroAcessoViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrimeiroAcesso(PrimeiroAcessoViewModel model)
        {
            if (await _context.Usuarios.AnyAsync()) return RedirectToAction(nameof(Login));
            if (!ModelState.IsValid) return View(model);

            var funcionario = await _context.Funcionarios.OrderBy(f => f.Id).FirstOrDefaultAsync();
            if (funcionario == null)
            {
                funcionario = new Funcionario
                {
                    Nome = model.Nome.Trim(),
                    Cargo = "gerente",
                    DataAdmissao = DateTime.Today,
                    Ativo = true
                };
                _context.Funcionarios.Add(funcionario);
                await _context.SaveChangesAsync();
            }

            _context.Usuarios.Add(new Usuario
            {
                FuncionarioId = funcionario.Id,
                Login = model.Login.Trim(),
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(model.Senha),
                Perfil = Perfis.Admin,
                Ativo = true,
                CriadoEm = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Info"] = "Administrador criado. Faça login para continuar.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied() => View();
    }
}
