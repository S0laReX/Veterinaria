using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Data;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var totalServicios =
                await _context.ServiciosVeterinarios.CountAsync();

            var totalMascotas =
                await _context.Mascotas.CountAsync();

            var totalCitas =
                await _context.Citas.CountAsync();

            var citasPendientes =
                await _context.Citas
                    .CountAsync(c => c.Estado == "Pendiente");

            var citasAtendidas =
                await _context.Citas
                    .CountAsync(c => c.Estado == "Atendida");

            var citasCanceladas =
                await _context.Citas
                    .CountAsync(c => c.Estado == "Cancelada");


            ViewBag.TotalServicios = totalServicios;
            ViewBag.TotalMascotas = totalMascotas;
            ViewBag.TotalCitas = totalCitas;
            ViewBag.TotalUsuarios = await _userManager.Users.CountAsync();

            ViewBag.CitasPendientes = citasPendientes;
            ViewBag.CitasAtendidas = citasAtendidas;
            ViewBag.CitasCanceladas = citasCanceladas;


            // =====================================================
            // CITAS POR MES
            // =====================================================

            var añoActual = DateTime.Now.Year;

            var citasPorMes = await _context.Citas
                .Where(c => c.FechaCita.Year == añoActual)
                .GroupBy(c => c.FechaCita.Month)
                .Select(g => new
                {
                    Mes = g.Key,
                    Total = g.Count()
                })
                .ToListAsync();

            int[] datosMeses = new int[12];

            foreach (var item in citasPorMes)
            {
                datosMeses[item.Mes - 1] = item.Total;
            }

            ViewBag.CitasPorMes = datosMeses;


            // =====================================================
            // CITAS RECIENTES
            // =====================================================

            var citasRecientes = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.ServicioVeterinario)
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaCita)
                .Take(5)
                .ToListAsync();

            ViewBag.CitasRecientes = citasRecientes;


            return View();
        }
    }
}