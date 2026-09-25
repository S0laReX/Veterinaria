using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Data;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Administrador,Cliente")]
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

        public async Task<IActionResult> Index([FromQuery] FiltrosListado filtros)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();
            var admin = User.IsInRole("Administrador");
            IQueryable<Cita> query = _context.Citas.AsNoTracking()
                .Include(c => c.Mascota).Include(c => c.ServicioVeterinario).Include(c => c.Usuario);
            if (!admin) query = query.Where(c => c.UsuarioId == usuario.Id);
            var texto = filtros.Buscar?.Trim();
            if (!string.IsNullOrWhiteSpace(texto))
                query = query.Where(c => c.Mascota!.Nombre.Contains(texto) || c.ServicioVeterinario!.Nombre.Contains(texto)
                    || (admin && (c.Usuario!.NombreCompleto.Contains(texto) || c.Usuario.Email!.Contains(texto))));
            if (!string.IsNullOrEmpty(filtros.Estado)) query = query.Where(c => c.Estado == filtros.Estado);
            if (filtros.Desde.HasValue) query = query.Where(c => c.FechaCita >= filtros.Desde.Value.Date);
            if (filtros.Hasta.HasValue) query = query.Where(c => c.FechaCita.Date <= filtros.Hasta.Value.Date);
            query = filtros.Orden switch
            {
                "fecha-asc" => query.OrderBy(c => c.FechaCita).ThenBy(c => c.Id),
                "precio-asc" => query.OrderBy(c => c.ServicioVeterinario!.Precio).ThenBy(c => c.FechaCita),
                "precio-desc" => query.OrderByDescending(c => c.ServicioVeterinario!.Precio).ThenBy(c => c.FechaCita),
                _ => query.OrderByDescending(c => c.FechaCita).ThenByDescending(c => c.Id)
            };
            ViewBag.Filtros = filtros;
            ViewBag.TipoListado = "citas";
            return View(await query.ToListAsync());
        }

        // =====================================================
        // CREAR
        // =====================================================

        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Create(int? servicioId)
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

            await CargarServiciosAsync(servicioId);

            return View(new Cita { ServicioVeterinarioId = servicioId ?? 0 });
        }

        // =====================================================
        // GUARDAR CITA
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Create([Bind("MascotaId,ServicioVeterinarioId,FechaCita")] Cita cita)
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

            ModelState.Remove(nameof(Cita.UsuarioId));
            if (!ModelState.IsValid)
            {
                ViewBag.Mascotas = new SelectList(
                    await _context.Mascotas
                        .Where(m => m.UsuarioId == usuario.Id)
                        .ToListAsync(),
                    "Id",
                    "Nombre",
                    cita.MascotaId);

                await CargarServiciosAsync(cita.ServicioVeterinarioId);

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

        private async Task CargarServiciosAsync(int? seleccionado)
        {
            var servicios = await _context.ServiciosVeterinarios.AsNoTracking().OrderBy(s => s.Nombre).ToListAsync();
            ViewBag.ServiciosDisponibles = servicios;
            ViewBag.Servicios = new SelectList(servicios.Select(s => new
            {
                s.Id, Etiqueta = $"{s.Nombre} · {Moneda.Bolivianos(s.Precio)}"
            }), "Id", "Etiqueta", seleccionado);
        }

        // =====================================================
        // ADMINISTRADOR - CAMBIAR ESTADO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(
            int id,
            string estado, string? returnUrl = null)
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

            TempData["Success"] = "El estado de la cita fue actualizado.";
            if (Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl!);
            return RedirectToAction(nameof(Index));
        }
    }
}