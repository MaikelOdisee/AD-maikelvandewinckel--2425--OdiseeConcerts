// Verwijder de licentie-header, die is niet nodig in je eigen code
#nullable disable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OdiseeConcerts.Models; // Zorg dat deze using aanwezig is

namespace OdiseeConcerts.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<CustomUser> _signInManager; // Gebruik CustomUser
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<CustomUser> signInManager, ILogger<LoginModel> logger) // Gebruik CustomUser
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "E-mailadres is verplicht.")] // Vertaald
            [EmailAddress(ErrorMessage = "Dit is geen geldig e-mailadres.")] // Vertaald
            [Display(Name = "E-mail")] // Vertaald
            public string Email { get; set; }

            [Required(ErrorMessage = "Wachtwoord is verplicht.")] // Vertaald
            [DataType(DataType.Password)]
            [Display(Name = "Wachtwoord")] // Vertaald
            public string Password { get; set; }

            [Display(Name = "Onthoud mij?")] // Vertaald
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Wis de bestaande externe cookie om een schoon inlogproces te garanderen
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Dit telt mislukte aanmeldpogingen niet mee voor accountvergrendeling.
                // Om accountvergrendeling bij mislukte wachtwoorden in te schakelen, stel lockoutOnFailure: true in
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Gebruiker ingelogd."); // Vertaald
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Gebruikersaccount is vergrendeld."); // Vertaald
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ongeldige inlogpoging."); // Vertaald
                    return Page();
                }
            }

            // Als we zover zijn gekomen, is er iets mislukt, toon het formulier opnieuw.
            return Page();
        }
    }
}