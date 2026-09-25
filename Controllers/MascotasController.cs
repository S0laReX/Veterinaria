using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Data;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class MascotasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MascotasController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Mascotas
        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var mascotas = await _context.Mascotas
                .Where(m => m.UsuarioId == usuario.Id)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            return View(mascotas);
        }

        // GET: Mascotas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Mascotas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mascota mascota)
        {
            if (!ModelState.IsValid)
                return View(mascota);

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            mascota.UsuarioId = usuario.Id;

            _context.Mascotas.Add(mascota);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Mascota registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Mascotas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.UsuarioId == usuario.Id);

            if (mascota == null)
                return NotFound();

            return View(mascota);
        }

        // POST: Mascotas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Mascota mascota)
        {
            if (id != mascota.Id)
                return NotFound();

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var mascotaDb = await _context.Mascotas
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.UsuarioId == usuario.Id);

            if (mascotaDb == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(mascota);

            mascotaDb.Nombre = mascota.Nombre;
            mascotaDb.Especie = mascota.Especie;
            mascotaDb.Raza = mascota.Raza;
            mascotaDb.Edad = mascota.Edad;
            mascotaDb.Sexo = mascota.Sexo;
            mascotaDb.Observaciones = mascota.Observaciones;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Mascota actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Mascotas/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.UsuarioId == usuario.Id);

            if (mascota == null)
                return NotFound();

            bool tieneCitas = await _context.Citas
                .AnyAsync(c => c.MascotaId == id);

            if (tieneCitas)
            {
                TempData["Error"] =
                    "No se puede eliminar la mascota porque tiene citas registradas.";

                return RedirectToAction(nameof(Index));
            }

            _context.Mascotas.Remove(mascota);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Mascota eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}