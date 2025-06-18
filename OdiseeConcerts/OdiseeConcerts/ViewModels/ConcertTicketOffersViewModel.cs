using OdiseeConcerts.ViewModels; // Nodig voor ConcertViewModel en TicketOfferDetailsViewModel
using System; // Nodig voor DateTime
using System.Collections.Generic; // Nodig voor IEnumerable

namespace OdiseeConcerts.ViewModels
{
    /// <summary>
    /// ViewModel voor de pagina die de details van een specifiek concert toont
    /// samen met alle bijbehorende ticketaanbiedingen.
    /// </summary>
    public class ConcertTicketOffersViewModel
    {
        // Concert details
        public int ConcertId { get; set; }
        public string Artist { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string ArtistPicture { get; set; } = string.Empty; // Voor de afbeelding van de artiest

        // Lijst van ticketaanbiedingen voor dit concert
        public IEnumerable<TicketOfferDetailsViewModel> TicketOffers { get; set; } = new List<TicketOfferDetailsViewModel>();
    }

    /// <summary>
    /// Sub-ViewModel voor de details van elke individuele ticketaanbieding,
    /// zoals weergegeven op de ConcertTicketOffers pagina.
    /// </summary>
    public class TicketOfferDetailsViewModel
    {
        public int Id { get; set; } // Het ID van de TicketOffer
        public string TicketType { get; set; } = string.Empty;
        public int NumTickets { get; set; }
        public decimal Price { get; set; }
    }
}
