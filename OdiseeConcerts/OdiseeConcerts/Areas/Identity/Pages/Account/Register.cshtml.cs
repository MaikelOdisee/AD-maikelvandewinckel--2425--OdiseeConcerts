// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using OdiseeConcerts.Models; // <-- DEZE MOET ER ZEKER STAAN!

namespace OdiseeConcerts.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IUserStore<CustomUser> _userStore;
        private readonly IUserEmailStore<CustomUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<CustomUser> userManager,
            IUserStore<CustomUser> userStore,
            SignInManager<CustomUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            // NIEUWE VELDEN VOOR CUSTOMUSER
            [Required(ErrorMessage = "Voornaam is verplicht.")]
            [Display(Name = "Voornaam")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Familienaam is verplicht.")]
            [Display(Name = "Familienaam")]
            public string LastName { get; set; }

            [Display(Name = "Lidkaartnummer")]
            // BELANGRIJK: De MemberCardNumberValidationAttribute voegen we pas later toe.
            // Nu nog NIET toevoegen, anders krijg je een build error.
            public string? MemberCardNumber { get; set; }

            // BESTAANDE VELDEN
            [Required(ErrorMessage = "E-mailadres is verplicht.")] // Vertaald
            [EmailAddress(ErrorMessage = "Geen geldig e-mailadres.")] // Vertaald
            [Display(Name = "E-mail")] // Vertaald
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Het {0} moet minstens {2} en maximaal {1} karakters lang zijn.", MinimumLength = 6)] // Vertaald
            [DataType(DataType.Password)]
            [Display(Name = "Wachtwoord")] // Vertaald
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Bevestig wachtwoord")] // Vertaald
            [Compare("Password", ErrorMessage = "Het wachtwoord en het bevestigingswachtwoord komen niet overeen.")] // Vertaald
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                ModelState.AddModelError(string.Empty, $"Ongeldige return URL '{ReturnUrl}'."); // Vertaald
            }

            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                // CAST NAAR CUSTOMUSER EN VUL DE NIEUWE VELDEN
                var customUser = (OdiseeConcerts.Models.CustomUser)user; // Zorg dat de namespace klopt!
                customUser.FirstName = Input.FirstName;
                customUser.LastName = Input.LastName;
                customUser.MemberCardNumber = Input.MemberCardNumber;

                await _userStore.SetUserNameAsync(customUser, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(customUser, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(customUser, Input.Password); // Gebruik customUser hier!

                if (result.Succeeded)
                {
                    _logger.LogInformation("Gebruiker heeft een nieuw account aangemaakt met wachtwoord."); // Vertaald

                    var userId = await _userManager.GetUserIdAsync(customUser); // Gebruik customUser hier
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(customUser); // Gebruik customUser hier
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Bevestig je e-mail", // Vertaald
                        $"Bevestig alstublieft uw account door <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>hier te klikken</a>."); // Vertaald

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(customUser, isPersistent: false); // Gebruik customUser hier
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private CustomUser CreateUser() // Moet CustomUser teruggeven
        {
            try
            {
                return Activator.CreateInstance<OdiseeConcerts.Models.CustomUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(CustomUser)}'. " +
                    $"Ensure that '{nameof(CustomUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<CustomUser> GetEmailStore() // Moet CustomUser zijn
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<CustomUser>)_userStore;
        }
    }
}