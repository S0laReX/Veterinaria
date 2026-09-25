using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Data;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize]
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CitasController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =====================================================
        // LISTADO
        // =====================================================

        public async Task<IActionResult> Index() { 
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();
            IQueryable<Cita> consulta = _context.Citas.Include(c => c.Mascota).Include(c => c.ServicioVeterinario).Include(c => c.Usuario);
            if (User.IsInRole("Cliente")) { consulta = consulta.Where(c => c.UsuarioId == usuario.Id);
            }
            var citas = await consulta.OrderByDescending(c => c.FechaCita).ToListAsync(); return View(citas);
        }

        // =====================================================
        // CREAR
        // =====================================================

        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Create()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var mascotas = await _context.Mascotas
                .Where(m => m.UsuarioId == usuario.Id)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            if (!mascotas.Any())
            {
                TempData["Error"] =
                    "Debe registrar al menos una mascota antes de solicitar una cita.";

                return RedirectToAction("Index", "Mascotas");
            }

            ViewBag.Mascotas = new SelectList(
                mascotas,
                "Id",
                "Nombre");

            ViewBag.Servicios = new SelectList(
                await _context.ServiciosVeterinarios
                    .OrderBy(s => s.Nombre)
                    .ToListAsync(),
                "Id",
                "Nombre");

            return View();
        }

        // =====================================================
        // GUARDAR CITA
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Create(Cita cita)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            // Seguridad: la mascota debe pertenecer al cliente
            bool mascotaPertenece =
                await _context.Mascotas.AnyAsync(m =>
                    m.Id == cita.MascotaId &&
                    m.UsuarioId == usuario.Id);

            if (!mascotaPertenece)
            {
                ModelState.AddModelError(
                    "MascotaId",
                    "La mascota seleccionada no pertenece al usuario autenticado.");
            }

            // Validación adicional de fecha
            if (cita.FechaCita < DateTime.Now)
            {
                ModelState.AddModelError(
                    "FechaCita",
                    "La fecha y hora de la cita no puede ser anterior al momento actual.");
            }

            // Comprobar servicio
            bool servicioExiste =
                await _context.ServiciosVeterinarios
                    .AnyAsync(s => s.Id == cita.ServicioVeterinarioId);

            if (!servicioExiste)
            {
                ModelState.AddModelError(
                    "ServicioVeterinarioId",
                    "El servicio seleccionado no existe.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Mascotas = new SelectList(
                    await _context.Mascotas
                        .Where(m => m.UsuarioId == usuario.Id)
                        .ToListAsync(),
                    "Id",
                    "Nombre",
                    cita.MascotaId);

                ViewBag.Servicios = new SelectList(
                    await _context.ServiciosVeterinarios
                        .OrderBy(s => s.Nombre)
                        .ToListAsync(),
                    "Id",
                    "Nombre",
                    cita.ServicioVeterinarioId);

                return View(cita);
            }

            cita.UsuarioId = usuario.Id;
            cita.Estado = "Pendiente";

            _context.Citas.Add(cita);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La cita fue registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // ADMINISTRADOR - CAMBIAR ESTADO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(
            int id,
            string estado)
        {
            var estadosPermitidos = new[]
            {
                "Pendiente",
                "Atendida",
                "Cancelada"
            };

            if (!estadosPermitidos.Contains(estado))
            {
                TempData["Error"] = "Estado no válido.";

                return RedirectToAction(nameof(Index));
            }

            var cita = await _context.Citas
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
                return NotFound();

            cita.Estado = estado;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "El estado de la cita fue actualizado.";

            return RedirectToAction(nameof(Index));
        }
    }
}