using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarcenariaVersatto.Data;
using MarcenariaVersatto.Models;

namespace MarcenariaVersatto.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            ViewData["TotalClientes"] = await _context.Clientes.CountAsync();
            ViewData["TotalMateriais"] = await _context.Materiais.CountAsync();
            ViewData["TotalOrcamentos"] = await _context.Orcamentos.CountAsync();
            ViewData["MateriaisAbaixoMinimo"] = await _context.Materiais
                .CountAsync(m => m.EstoqueAtual < m.EstoqueMinimo);

            // Lista curta de alertas (UC10) só para o administrador
            if (User.IsInRole(Perfis.Admin))
            {
                ViewData["Alertas"] = await _context.Materiais
                    .Where(m => m.EstoqueAtual < m.EstoqueMinimo)
                    .OrderBy(m => m.EstoqueAtual - m.EstoqueMinimo)
                    .Take(5)
                    .ToListAsync();
            }
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
