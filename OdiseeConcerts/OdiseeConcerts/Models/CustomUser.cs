using Microsoft.AspNetCore.Identity; // Nodig voor IdentityUser
using System.ComponentModel.DataAnnotations; // Nodig voor [Display], [Required]

namespace OdiseeConcerts.Models
{
    // Breid de standaard IdentityUser uit met specifieke eigenschappen voor je applicatie.
    public class CustomUser : IdentityUser
    {
        [PersonalData] // Markeer als persoonlijke data voor AVG-doeleinden
        [Display(Name = "Voornaam")]
        [Required(ErrorMessage = "Voornaam is verplicht.")] // Voornaam is verplicht
        public string FirstName { get; set; } = string.Empty; // Initialiseren om CS8618 op te lossen

        [PersonalData]
        [Display(Name = "Familienaam")]
        [Required(ErrorMessage = "Familienaam is verplicht.")] // Familienaam is verplicht
        public string LastName { get; set; } = string.Empty; // Initialiseren om CS8618 op te lossen

        [PersonalData]
        [Display(Name = "Lidkaartnummer")]
        public string? MemberCardNumber { get; set; } // Kan nullable zijn, dus geen string.Empty nodig, maar kan voor consistentie

        [PersonalData]
        [Display(Name = "Heeft Ledenkaart")]
        public bool HasMemberCard { get; set; } // NIEUWE property toegevoegd om CS1061 op te lossen
    }
}