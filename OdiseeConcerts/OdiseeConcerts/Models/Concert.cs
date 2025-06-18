using System.ComponentModel.DataAnnotations;

namespace OdiseeConcerts.Models
{
    public class Concert : BaseEntity
    {
        [Required]
        [Display(Name = "Artiest")] // TOEGEVOEGD: Vertaalde display naam
        public string Artist { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Locatie")] // TOEGEVOEGD: Vertaalde display naam
        public string Location { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Datum")] // TOEGEVOEGD: Vertaalde display naam
        public DateTime Date { get; set; }

        // Navigatie property voor gerelateerde TicketOffers
        public ICollection<TicketOffer>? TicketOffers { get; set; }
    }
}
