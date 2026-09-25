using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Veterinaria.Models;
using Veterinaria.Data;
using Microsoft.EntityFrameworkCore;

namespace Veterinaria.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            return View(await _context.ServiciosVeterinarios.AsNoTracking().OrderBy(s => s.Nombre).ToListAsync());
        }

        public async Task<IActionResult> Servicios([FromQuery] FiltrosListado filtros)
        {
            ViewBag.Filtros = filtros;
            ViewBag.TipoListado = "servicios";
            return View(await filtros.AplicarServicios(_context.ServiciosVeterinarios.AsNoTracking()).ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
