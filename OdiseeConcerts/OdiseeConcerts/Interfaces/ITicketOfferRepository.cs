using OdiseeConcerts.Models; // Nodig voor TicketOffer model
using System.Threading.Tasks; // TOEGEVOEGD: Nodig voor Task

namespace OdiseeConcerts.Interfaces
{
    // Interface voor de TicketOffer Repository.
    // Definieert de methoden die een TicketOffer Repository moet implementeren.
    public interface ITicketOfferRepository
    {
        /// <summary>
        /// Haalt een specifieke ticketaanbieding op basis van het ID.
        /// LET OP: Deze methode laadt GEEN gerelateerd Concert in.
        /// </summary>
        /// <param name="id">Het ID van de ticketaanbieding.</param>
        /// <returns>Het TicketOffer object, of null als niet gevonden.</returns>
        TicketOffer? GetTicketOfferById(int id);

        /// <summary>
        /// Haalt een specifieke ticketaanbieding op basis van het ID, inclusief het gerelateerde Concert.
        /// </summary>
        /// <param name="id">Het ID van de ticketaanbieding.</param>
        /// <returns>Het TicketOffer object met het geladen Concert, of null als niet gevonden.</returns>
        TicketOffer? GetTicketOfferWithConcertById(int id);

        /// <summary>
        /// Slaat de wijzigingen van een TicketOffer op in de database.
        /// </summary>
        /// <param name="ticketOffer">Het TicketOffer object waarvan de wijzigingen moeten worden opgeslagen.</param>
        Task UpdateTicketOffer(TicketOffer ticketOffer); // AANGEPAST: Methode is nu async Task
    }
}