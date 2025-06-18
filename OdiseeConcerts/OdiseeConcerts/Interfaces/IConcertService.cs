using System.Collections.Generic;
using OdiseeConcerts.ViewModels; // Nodig voor ConcertViewModel en ConcertTicketOffersViewModel

namespace OdiseeConcerts.Interfaces
{
    // Interface voor de Concert Service
    // Definieert de methoden die een Concert Service moet implementeren.
    public interface IConcertService
    {
        /// <summary>
        /// Haalt alle concerten op en converteert deze naar ConcertViewModel objecten.
        /// </summary>
        /// <returns>Een IEnumerable van ConcertViewModel objecten.</returns>
        IEnumerable<ConcertViewModel> GetAllConcerts();

        /// <summary>
        /// Haalt de details van een specifiek concert op, inclusief alle bijbehorende ticketaanbiedingen,
        /// en mapt deze naar een ConcertTicketOffersViewModel.
        /// </summary>
        /// <param name="concertId">Het ID van het concert.</param>
        /// <returns>Een ConcertTicketOffersViewModel met concert- en ticketaanbiedingsdetails, of null als niet gevonden.</returns>
        ConcertTicketOffersViewModel? GetConcertDetailsWithOffers(int concertId);
    }
}
