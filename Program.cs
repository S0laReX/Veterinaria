using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Data;
using Veterinaria.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// BASE DE DATOS
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ============================================================
// IDENTITY
// ============================================================

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        // No exigiremos confirmación de correo en este proyecto.
        options.SignIn.RequireConfirmedAccount = false;

        // Reglas de contraseña.
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // No permitir dos usuarios con el mismo correo.
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configuración de cookie.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// ============================================================
// MVC + RAZOR PAGES
// ============================================================

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// ============================================================
// PIPELINE HTTP
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Primero Authentication y después Authorization.
app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// RUTAS MVC
// ============================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Necesario para las páginas de Identity:
// Login, Register, Logout, Manage, etc.
app.MapRazorPages();

// ============================================================
// SEEDING
// ============================================================

using (var scope = app.Services.CreateScope())
{
    try
    {
        await DbInitializer.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger =
            scope.ServiceProvider
                .GetRequiredService<ILogger<Program>>();

        logger.LogError(
            ex,
            "Ocurrió un error inicializando la base de datos.");

        throw;
    }
}

app.Run();