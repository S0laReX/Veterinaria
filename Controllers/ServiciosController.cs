using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Data;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    /*
     * Solo los usuarios pertenecientes al rol Administrador
     * podrán acceder al CRUD de servicios.
     */
    [Authorize(Roles = "Administrador")]
    public class ServiciosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiciosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ====================================================
        // GET: Servicios
        // ====================================================

        public async Task<IActionResult> Index([FromQuery] FiltrosListado filtros)
        {
            ViewBag.Filtros = filtros;
            ViewBag.TipoListado = "servicios";
            var query = filtros.AplicarServicios(_context.ServiciosVeterinarios.AsNoTracking());
            return View(await query.ToListAsync());
        }

        // ====================================================
        // GET: Servicios/Details/5
        //
        // IMPORTANTE:
        // Devuelve solamente una vista parcial.
        // Esta información será cargada dentro del Modal.
        // ====================================================

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var servicio = await _context.ServiciosVeterinarios
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servicio == null)
                return NotFound();

            return PartialView(
                "_DetailsModalContent",
                servicio);
        }

        // ====================================================
        // GET: Servicios/Create
        // ====================================================

        public IActionResult Create()
        {
            return View();
        }

        // ====================================================
        // POST: Servicios/Create
        // ====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nombre,Descripcion,Precio")]
            ServicioVeterinario servicio)
        {
            if (!ModelState.IsValid)
                return View(servicio);

            _context.Add(servicio);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "El servicio fue creado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // GET: Servicios/Edit/5
        // ====================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var servicio =
                await _context.ServiciosVeterinarios.FindAsync(id);

            if (servicio == null)
                return NotFound();

            return View(servicio);
        }

        // ====================================================
        // POST: Servicios/Edit/5
        // ====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Nombre,Descripcion,Precio")]
            ServicioVeterinario servicio)
        {
            if (id != servicio.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(servicio);

            try
            {
                _context.Update(servicio);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "El servicio fue actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServicioVeterinarioExists(servicio.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // GET: Servicios/Delete/5
        // ====================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var servicio =
                await _context.ServiciosVeterinarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

            if (servicio == null)
                return NotFound();

            return View(servicio);
        }

        // ====================================================
        // POST: Servicios/Delete/5
        // ====================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var servicio =
                await _context.ServiciosVeterinarios
                    .FindAsync(id);

            if (servicio == null)
                return RedirectToAction(nameof(Index));

            if (await _context.Citas.AnyAsync(c => c.ServicioVeterinarioId == id))
            {
                TempData["Error"] = "No se puede eliminar el servicio porque existen citas asociadas.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.ServiciosVeterinarios.Remove(servicio);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "El servicio fue eliminado correctamente.";
            }
            catch (DbUpdateException)
            {
                /*
                 * Puede ocurrir si existen citas utilizando
                 * este servicio, debido al DeleteBehavior.Restrict.
                 */
                TempData["Error"] =
                    "No se puede eliminar el servicio porque existen citas asociadas.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ServicioVeterinarioExists(int id)
        {
            return _context.ServiciosVeterinarios
                .Any(s => s.Id == id);
        }
    }
}
