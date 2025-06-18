using System.ComponentModel.DataAnnotations; // Nodig voor [Required] en [Range]

namespace OdiseeConcerts.ViewModels
{
    /// <summary>
    /// ViewModel voor het bijwerken van een TicketOffer, voornamelijk het aantal tickets.
    /// </summary>
    public class TicketOfferUpdateViewModel
    {
        [Required]
        public int Id { get; set; } // Het ID van de TicketOffer die moet worden bijgewerkt

        [Required(ErrorMessage = "Het aantal tickets is verplicht.")]
        [Range(0, int.MaxValue, ErrorMessage = "Het aantal tickets moet een positief getal zijn of nul.")]
        [Display(Name = "Nieuw aantal tickets")]
        public int NewNumTickets { get; set; }

        // Optioneel: Voeg andere properties toe als deze ook bijgewerkt moeten worden via deze ViewModel
        // public decimal NewPrice { get; set; }
    }
}

