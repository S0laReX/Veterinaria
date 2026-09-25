using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Veterinaria.Models;
namespace Veterinaria.Areas.Identity.Pages.Account;
[AllowAnonymous]
public class LoginModel(SignInManager<ApplicationUser> signIn) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public class InputModel
    {
        [Required, EmailAddress, Display(Name = "Correo electrónico")]
        public string Email { get; set; } = "";
        [Required, DataType(DataType.Password), Display(Name = "Contraseña")]
        public string Password { get; set; } = "";
        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }
    }
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return Page();
        var result = await signIn.PasswordSignInAsync(Input.Email.Trim(), Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded) return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
        if (result.RequiresTwoFactor) return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
        ModelState.AddModelError("", result.IsLockedOut ? "Cuenta bloqueada temporalmente. Inténtalo más tarde." : "Correo o contraseña incorrectos.");
        return Page();
    }
}
