using System.ComponentModel.DataAnnotations; // Nodig voor [Display], [DataType]
using OdiseeConcerts.Models; // Nodig voor Order, TicketOffer, Concert, CustomUser modellen

namespace OdiseeConcerts.ViewModels
{
    /// <summary>
    /// ViewModel voor het weergeven van besteldetails.
    /// </summary>
    public class OrderViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Gebruiker E-mail")]
        public string UserEmail { get; set; } = string.Empty;

        [Display(Name = "Gebruiker Naam")]
        public string UserFullName { get; set; } = string.Empty;

        [Display(Name = "Aantal Tickets")]
        public int NumTickets { get; set; }

        [Display(Name = "Totale Prijs")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Betaald")]
        public bool Paid { get; set; }

        [Display(Name = "Korting Toegepast")]
        public bool DiscountApplied { get; set; }

        [Display(Name = "Ticket Type")]
        public string TicketType { get; set; } = string.Empty;

        [Display(Name = "Prijs per Ticket")]
        [DataType(DataType.Currency)]
        public decimal PricePerTicket { get; set; }

        [Display(Name = "Concert Artiest")]
        public string ConcertArtist { get; set; } = string.Empty;

        [Display(Name = "Concert Locatie")]
        public string ConcertLocation { get; set; } = string.Empty;

        [Display(Name = "Concert Datum")]
        public DateTime ConcertDate { get; set; }
    }
}
