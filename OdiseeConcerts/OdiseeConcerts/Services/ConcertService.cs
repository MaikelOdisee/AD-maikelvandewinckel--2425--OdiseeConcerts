using System.Collections.Generic;
using System.Linq; // Nodig voor .Select() en eventueel andere LINQ-methoden
using OdiseeConcerts.Interfaces; // Nodig voor IConcertService en IConcertRepository
using OdiseeConcerts.ViewModels; // Nodig voor ConcertViewModel
using OdiseeConcerts.Models; // Nodig voor Concert model (voor mapping)

namespace OdiseeConcerts.Services
{
    // Implementeert de IConcertService interface.
    // Verantwoordelijk voor de bedrijfslogica gerelateerd aan Concerten.
    public class ConcertService : IConcertService
    {
        private readonly IConcertRepository _concertRepository;

        /// <summary>
        /// Constructor voor ConcertService.
        /// Injecteert de IConcertRepository om toegang te krijgen tot data.
        /// </summary>
        /// <param name="concertRepository">De repository voor concertdata.</param>
        public ConcertService(IConcertRepository concertRepository)
        {
            _concertRepository = concertRepository;
        }

        /// <summary>
        /// Haalt alle concerten op via de repository en mappeert deze naar ConcertViewModel objecten.
        /// </summary>
        /// <returns>Een IEnumerable van ConcertViewModel objecten.</returns>
        public IEnumerable<ConcertViewModel> GetAllConcerts()
        {
            // Haal de Concert-objecten (inclusief TicketOffers) op van de repository.
            var concerts = _concertRepository.GetConcertsWithTicketOffers();

            // Mappeer de Concert-objecten naar ConcertViewModel-objecten.
            // Dit is waar we de benodigde data selecteren en logica uitvoeren
            // zoals het berekenen van beschikbare tickets en knopteksten.
            return concerts.Select(c => new ConcertViewModel
            {
                Id = c.Id,
                Artist = c.Artist,
                Location = c.Location,
                Date = c.Date,
                // Geef de TicketOffers rechtstreeks door aan het ViewModel.
                // De logica voor AvailableTickets, ButtonText en ButtonClass
                // wordt afgehandeld binnen de ConcertViewModel zelf.
                TicketOffers = c.TicketOffers
            }).ToList(); // Converteer naar een lijst om de enumeratie te forceren.
        }
    }
}
