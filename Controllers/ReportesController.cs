using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Veterinaria.Data;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =====================================================
        // PÁGINA PRINCIPAL
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var clientes = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            return View(clientes);
        }

        // =====================================================
        // REPORTE 1
        // TODAS LAS CITAS
        // =====================================================

        public async Task<IActionResult> CitasGeneral()
        {
            var citas = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.Usuario)
                .Include(c => c.ServicioVeterinario)
                .OrderBy(c => c.FechaCita)
                .ToListAsync();

            QuestPDF.Settings.License =
                LicenseType.Community;

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());

                    page.Margin(30);

                    page.Header()
                        .Text("REPORTE GENERAL DE CITAS")
                        .Bold()
                        .FontSize(20);

                    page.Content()
                        .PaddingVertical(20)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.2f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeader)
                                    .Text("Mascota");

                                header.Cell().Element(CellHeader)
                                    .Text("Cliente");

                                header.Cell().Element(CellHeader)
                                    .Text("Servicio");

                                header.Cell().Element(CellHeader)
                                    .Text("Fecha");

                                header.Cell().Element(CellHeader)
                                    .Text("Estado");
                            });

                            foreach (var cita in citas)
                            {
                                table.Cell()
                                    .Text(cita.Mascota?.Nombre ?? "");

                                table.Cell()
                                    .Text(cita.Usuario?.Email ?? "");

                                table.Cell()
                                    .Text(cita.ServicioVeterinario?.Nombre ?? "");

                                table.Cell()
                                    .Text(cita.FechaCita.ToString("dd/MM/yyyy HH:mm"));

                                table.Cell()
                                    .Text(cita.Estado);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Veterinaria - ");
                            text.Span(DateTime.Now.ToString("dd/MM/yyyy"));
                        });
                });
            });

            byte[] pdf = documento.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                "Reporte_General_Citas.pdf");
        }

        // =====================================================
        // REPORTE 2
        // CITAS DE CLIENTE
        // =====================================================

        public async Task<IActionResult> CitasPorCliente(
            string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                return BadRequest("Debe seleccionar un cliente.");

            var usuario = await _userManager.FindByIdAsync(usuarioId);

            if (usuario == null)
                return NotFound();

            var citas = await _context.Citas
                .Where(c => c.UsuarioId == usuarioId)
                .Include(c => c.Mascota)
                .Include(c => c.ServicioVeterinario)
                .OrderBy(c => c.FechaCita)
                .ToListAsync();

            QuestPDF.Settings.License =
                LicenseType.Community;

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());

                    page.Margin(30);

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("REPORTE DE CITAS POR CLIENTE")
                                .Bold()
                                .FontSize(20);

                            column.Item()
                                .Text($"Cliente: {usuario.Email}")
                                .FontSize(12);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell()
                                    .Element(CellHeader)
                                    .Text("Mascota");

                                header.Cell()
                                    .Element(CellHeader)
                                    .Text("Servicio");

                                header.Cell()
                                    .Element(CellHeader)
                                    .Text("Fecha");

                                header.Cell()
                                    .Element(CellHeader)
                                    .Text("Estado");
                            });

                            foreach (var cita in citas)
                            {
                                table.Cell()
                                    .Text(cita.Mascota?.Nombre ?? "");

                                table.Cell()
                                    .Text(cita.ServicioVeterinario?.Nombre ?? "");

                                table.Cell()
                                    .Text(cita.FechaCita.ToString("dd/MM/yyyy HH:mm"));

                                table.Cell()
                                    .Text(cita.Estado);
                            }
                        });
                });
            });

            byte[] pdf = documento.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Citas_{usuario.Email}.pdf");
        }

        // =====================================================
        // REPORTE 3
        // SERVICIOS MÁS SOLICITADOS
        // =====================================================

        public async Task<IActionResult> ServiciosMasSolicitados()
        {
            var datos = await _context.Citas
                .Include(c => c.ServicioVeterinario)
                .GroupBy(c => new
                {
                    c.ServicioVeterinarioId,
                    Nombre = c.ServicioVeterinario!.Nombre
                })
                .Select(g => new
                {
                    Servicio = g.Key.Nombre,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToListAsync();

            QuestPDF.Settings.License =
                LicenseType.Community;

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    page.Margin(40);

                    page.Header()
                        .Text("SERVICIOS MÁS SOLICITADOS")
                        .Bold()
                        .FontSize(20);

                    page.Content()
                        .PaddingVertical(20)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell()
                                    .Element(CellHeader)
                                    .Text("Servicio");

                                header.Cell()
                                    .Element(CellHeader)
                                    .Text("Cantidad de citas");
                            });

                            foreach (var item in datos)
                            {
                                table.Cell()
                                    .Text(item.Servicio);

                                table.Cell()
                                    .Text(item.Cantidad.ToString());
                            }
                        });
                });
            });

            byte[] pdf = documento.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                "Servicios_Mas_Solicitados.pdf");
        }

        private static IContainer CellHeader(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten2)
                .Padding(5)
                .BorderBottom(1);
        }
    }
}