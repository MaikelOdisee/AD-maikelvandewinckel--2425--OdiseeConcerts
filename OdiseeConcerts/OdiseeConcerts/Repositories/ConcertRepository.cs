using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Nodig voor .Include() en .ToList()
using OdiseeConcerts.Data; // Nodig voor ApplicationDbContext
using OdiseeConcerts.Models; // Nodig voor Concert model
using OdiseeConcerts.Interfaces; // Nodig voor IConcertRepository

namespace OdiseeConcerts.Repositories
{
    // Implementeert de IConcertRepository interface.
    // Verantwoordelijk voor data-toegang gerelateerd aan Concerten.
    public class ConcertRepository : IConcertRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor voor ConcertRepository.
        /// Injecteert de ApplicationDbContext om toegang te krijgen tot de database.
        /// </summary>
        /// <param name="context">De database context.</param>
        public ConcertRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Haalt alle concerten op uit de database, inclusief hun gerelateerde TicketOffers.
        /// </summary>
        /// <returns>Een IEnumerable van Concert objecten, elk met hun TicketOffers geladen.</returns>
        public IEnumerable<Concert> GetConcertsWithTicketOffers()
        {
            // Gebruik .Include() om de gerelateerde TicketOffers mee te laden
            // Dit zorgt ervoor dat wanneer een Concert wordt opgehaald, de TicketOffers ook beschikbaar zijn.
            return _context.Concerts.Include(c => c.TicketOffers).ToList();
        }
    }
}
