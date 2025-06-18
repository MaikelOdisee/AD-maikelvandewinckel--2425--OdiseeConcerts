using OdiseeConcerts.Models; // Zorg ervoor dat dit aanwezig is om Concert te kunnen gebruiken

namespace OdiseeConcerts.Interfaces
{
    // Interface voor de Concert Repository
    // Definieert de methoden die een Concert Repository moet implementeren.
    public interface IConcertRepository
    {
        /// <summary>
        /// Haalt alle concerten op, inclusief hun gerelateerde TicketOffers.
        /// </summary>
        /// <returns>Een IEnumerable van Concert objecten, elk met hun TicketOffers geladen.</returns>
        IEnumerable<Concert> GetConcertsWithTicketOffers();
    }
}

