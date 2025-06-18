using OdiseeConcerts.Data; // Nodig voor ApplicationDbContext
using OdiseeConcerts.Models; // Nodig voor TicketOffer model
using OdiseeConcerts.Interfaces; // Nodig voor ITicketOfferRepository
using Microsoft.EntityFrameworkCore; // Nodig voor .Include() en .FirstOrDefault(), .SaveChangesAsync()
using System.Threading.Tasks; // TOEGEVOEGD: Nodig voor Task

namespace OdiseeConcerts.Repositories
{
    // Implementeert de ITicketOfferRepository interface.
    // Verantwoordelijk voor data-toegang gerelateerd aan TicketOffers.
    public class TicketOfferRepository : ITicketOfferRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor voor TicketOfferRepository.
        /// Injecteert de ApplicationDbContext om toegang te krijgen tot de database.
        /// </summary>
        /// <param name="context">De database context.</param>
        public TicketOfferRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Haalt een specifieke TicketOffer op uit de database op basis van het ID.
        /// LET OP: Deze methode laadt GEEN gerelateerd Concert in.
        /// </summary>
        /// <param name="id">Het ID van de ticketaanbieding.</param>
        /// <returns>Het TicketOffer object, of null als niet gevonden.</returns>
        public TicketOffer? GetTicketOfferById(int id)
        {
            // Gebruik Find() voor zoeken op Primary Key, dit is geoptimaliseerd.
            return _context.TicketOffers.Find(id);
        }

        /// <summary>
        /// Haalt een specifieke ticketaanbieding op basis van het ID, inclusief het gerelateerde Concert.
        /// </summary>
        /// <param name="id">Het ID van de ticketaanbieding.</param>
        /// <returns>Het TicketOffer object met het geladen Concert, of null als niet gevonden.</returns>
        public TicketOffer? GetTicketOfferWithConcertById(int id)
        {
            // Gebruik Include() om het gerelateerde Concert mee te laden.
            return _context.TicketOffers
                           .Include(to => to.Concert)
                           .FirstOrDefault(to => to.Id == id);
        }

        /// <summary>
        /// Slaat de wijzigingen van een TicketOffer op in de database.
        /// Dit wordt gebruikt om bijvoorbeeld het aantal tickets bij te werken.
        /// </summary>
        /// <param name="ticketOffer">Het TicketOffer object waarvan de wijzigingen moeten worden opgeslagen.</param>
        public async Task UpdateTicketOffer(TicketOffer ticketOffer) // AANGEPAST: Methode is nu async Task
        {
            // Entity Framework Core houdt de staat van entiteiten bij.
            // Als het 'ticketOffer' object al is getracked door de context en gewijzigd,
            // zal SaveChanges() de wijzigingen opslaan.
            // Voor de zekerheid kunnen we ook expliciet de staat instellen:
            _context.Entry(ticketOffer).State = EntityState.Modified;
            await _context.SaveChangesAsync(); // AANGEPAST: Gebruik SaveChangesAsync
        }
    }
}