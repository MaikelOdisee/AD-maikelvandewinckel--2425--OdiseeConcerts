using System.ComponentModel.DataAnnotations; // Nodig voor [Required], [Range]
using System; // Nodig voor DateTime

namespace OdiseeConcerts.ViewModels
{
    /// <summary>
    /// ViewModel voor het formulier op de 'Koop Tickets' pagina (Concerts/Buy).
    /// Dit model is gestroomlijnd om alleen de essentiële gebruikersinvoer te ontvangen (aantal tickets).
    /// Prijsberekening en -validatie gebeuren volledig aan de serverzijde.
    /// </summary>
    public class OrderFormViewModel
    {
        // Eigenschappen van Concert en TicketOffer die nodig zijn voor WEERGAVE op het formulier (GET-verzoek)
        // Deze worden niet door de gebruiker gewijzigd, maar zijn nodig om de pagina te vullen.
        public int ConcertId { get; set; }
        [Display(Name = "Artiest")]
        public string Artist { get; set; } = string.Empty;
        [Display(Name = "Locatie")]
        public string Location { get; set; } = string.Empty;
        [Display(Name = "Datum")]
        public DateTime Date { get; set; }

        // NIEUW: Property voor de artiestenafbeelding URL
        public string ArtistPicture { get; set; } = string.Empty;

        public int TicketOfferId { get; set; }
        [Display(Name = "Type Ticket")]
        public string TicketDescription { get; set; } = string.Empty;

        [Display(Name = "Prijs per Ticket")]
        [DataType(DataType.Currency)]
        // Belangrijk: Deze property wordt alleen gebruikt voor WEERGAVE op de GET-pagina.
        // Hij wordt NIET via een hidden input teruggestuurd in de POST.
        public decimal PricePerTicket { get; set; }

        // Input velden van de gebruiker (deze worden wel teruggestuurd via POST)
        [Required(ErrorMessage = "Aantal tickets is verplicht.")]
        [Range(1, 10, ErrorMessage = "U kunt tussen 1 en 10 tickets bestellen.")]
        [Display(Name = "Aantal Tickets")]
        public int NumberOfTickets { get; set; } = 1; // Standaard 1

        [Display(Name = "Ledenkaart")]
        // Deze property wordt gebruikt om de korting server-side te berekenen,
        // en kan eventueel ook via een hidden field van de GET-pagina komen.
        public bool HasMemberCard { get; set; }

        [Display(Name = "Totaalprijs")]
        [DataType(DataType.Currency)]
        // Belangrijk: Deze property wordt alleen gebruikt voor WEERGAVE op de GET-pagina (dynamisch via JS).
        // Hij wordt NIET via een hidden input teruggestuurd in de POST.
        public decimal TotalPrice { get; set; }

        // Voor de voorraadbeperking in de logica
        public int AvailableTicketsInOffer { get; set; } // Hoeveel tickets van dit type zijn er nog beschikbaar

        /// <summary>
        /// Het ID van de gebruiker die de bestelling plaatst.
        /// Dit wordt vanuit de controller gevuld en meegestuurd naar de service.
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}
