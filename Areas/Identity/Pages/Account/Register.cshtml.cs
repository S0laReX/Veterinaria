using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Veterinaria.Models;

namespace Veterinaria.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public class InputModel
    {
        [Required(ErrorMessage = "Escribe tu nombre completo."), StringLength(150), Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = "";
        [Required(ErrorMessage = "Escribe tu correo."), EmailAddress, Display(Name = "Correo electrónico")]
        public string Email { get; set; } = "";
        [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password), Display(Name = "Contraseña")]
        public string Password { get; set; } = "";
        [DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden."), Display(Name = "Confirmar contraseña")]
        public string ConfirmPassword { get; set; } = "";
    }
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return Page();
        var user = new ApplicationUser { UserName = Input.Email.Trim(), Email = Input.Email.Trim(), NombreCompleto = Input.NombreCompleto.Trim() };
        var result = await users.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            var role = await users.AddToRoleAsync(user, "Cliente");
            if (!role.Succeeded)
            {
                await users.DeleteAsync(user);
                ModelState.AddModelError("", "No se pudo crear tu cuenta. Inténtalo nuevamente.");
                return Page();
            }
            await signIn.SignInAsync(user, isPersistent: false);
            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
        }
        foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
        return Page();
    }
}
