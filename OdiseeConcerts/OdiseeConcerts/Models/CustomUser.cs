using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations; // <-- Zorg dat deze using er staat!

namespace OdiseeConcerts.Models // <-- Zorg dat deze namespace overeenkomt met de map en projectnaam!
{
    public class CustomUser : IdentityUser // <-- Deze definitie is cruciaal!
    {
        [Required(ErrorMessage = "Voornaam is verplicht.")]
        [Display(Name = "Voornaam")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Familienaam is verplicht.")]
        [Display(Name = "Familienaam")]
        public string LastName { get; set; }

        [Display(Name = "Lidkaartnummer")]
        public string? MemberCardNumber { get; set; } // Let op de '?' voor nullable string
    }
}