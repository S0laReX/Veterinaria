using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Models;

namespace Veterinaria.Data
{
    public static class DbInitializer
    {
        private const string RolAdministrador = "Administrador";
        private const string RolCliente = "Cliente";

        // Usuario inicial para desarrollo / proyecto académico.
        private const string AdminEmail = "admin@veterinaria.com";
        private const string AdminPassword = "Admin123*";

        public static async Task InitializeAsync(IServiceProvider services)
        {
    

            var roleManager =
                services.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                services.GetRequiredService<UserManager<ApplicationUser>>();

            /*
             * Aplica cualquier migración pendiente.
             *
             * Es cómodo para un proyecto académico/desarrollo.
             * En sistemas empresariales suele manejarse la migración
             * durante el despliegue.
             */

            // ----------------------------------------
            // Crear rol Administrador
            // ----------------------------------------

            if (!await roleManager.RoleExistsAsync(RolAdministrador))
            {
                var resultado = await roleManager.CreateAsync(
                    new IdentityRole(RolAdministrador));

                if (!resultado.Succeeded)
                {
                    throw new InvalidOperationException(
                        "No se pudo crear el rol Administrador.");
                }
            }

            // ----------------------------------------
            // Crear rol Cliente
            // ----------------------------------------

            if (!await roleManager.RoleExistsAsync(RolCliente))
            {
                var resultado = await roleManager.CreateAsync(
                    new IdentityRole(RolCliente));

                if (!resultado.Succeeded)
                {
                    throw new InvalidOperationException(
                        "No se pudo crear el rol Cliente.");
                }
            }

            // ----------------------------------------
            // Crear usuario administrador
            // ----------------------------------------

            var admin = await userManager.FindByEmailAsync(AdminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    NombreCompleto = "Administrador Veterinaria",

                    // Para este proyecto no exigiremos confirmación
                    // de correo electrónico.
                    EmailConfirmed = true
                };

                var resultadoUsuario =
                    await userManager.CreateAsync(
                        admin,
                        AdminPassword);

                if (!resultadoUsuario.Succeeded)
                {
                    var errores = string.Join(
                        Environment.NewLine,
                        resultadoUsuario.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"No se pudo crear el administrador:{Environment.NewLine}{errores}");
                }
            }

            // ----------------------------------------
            // Asignar Administrador al rol
            // ----------------------------------------

            if (!await userManager.IsInRoleAsync(
                    admin,
                    RolAdministrador))
            {
                var resultadoRol =
                    await userManager.AddToRoleAsync(
                        admin,
                        RolAdministrador);

                if (!resultadoRol.Succeeded)
                {
                    throw new InvalidOperationException(
                        "No se pudo asignar el rol Administrador.");
                }
            }
        }
    }
}