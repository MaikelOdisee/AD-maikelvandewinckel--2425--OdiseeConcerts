using OdiseeConcerts.Data; // Nodig voor ApplicationDbContext
using OdiseeConcerts.Models; // Nodig voor Order model
using OdiseeConcerts.Interfaces; // Nodig voor IOrderRepository
using Microsoft.EntityFrameworkCore; // Nodig voor .Include(), .FirstOrDefault(), .SaveChangesAsync()
using System.Threading.Tasks; // Nodig voor Task

namespace OdiseeConcerts.Repositories
{
    // Implementeert de IOrderRepository interface.
    // Verantwoordelijk voor data-toegang gerelateerd aan Bestellingen.
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor voor OrderRepository.
        /// Injecteert de ApplicationDbContext om toegang te krijgen tot de database.
        /// </summary>
        /// <param name="context">De database context.</param>
        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Voegt een nieuwe bestelling toe aan de database en slaat de wijzigingen op.
        /// </summary>
        /// <param name="order">Het Order object dat moet worden toegevoegd.</param>
        public async Task AddOrder(Order order) // AANGEPAST: Methode is nu async Task
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // AANGEPAST: Gebruik SaveChangesAsync
        }

        /// <summary>
        /// Haalt een specifieke bestelling op uit de database op basis van het ID.
        /// LET OP: Deze methode laadt GEEN gerelateerde entiteiten (zoals TicketOffer, Concert, User) in.
        /// </summary>
        /// <param name="id">Het ID van de bestelling.</param>
        /// <returns>Het Order object, of null als niet gevonden.</returns>
        public Order? GetOrderById(int id)
        {
            // Gebruik Find() voor zoeken op Primary Key.
            return _context.Orders.Find(id);
        }

        /// <summary>
        /// Haalt een specifieke bestelling op uit de database op basis van het ID,
        /// inclusief gerelateerde TicketOffer, Concert (via TicketOffer) en User.
        /// </summary>
        /// <param name="id">Het ID van de bestelling.</param>
        /// <returns>Het Order object met alle geladen details, of null als niet gevonden.</returns>
        public Order? GetOrderWithDetailsById(int id)
        {
            return _context.Orders
                           .Include(o => o.TicketOffer)
                               .ThenInclude(to => to.Concert) // Laad Concert via TicketOffer
                           .Include(o => o.User) // Laad de gerelateerde CustomUser
                           .FirstOrDefault(o => o.Id == id);
        }

        /// <summary>
        /// Werkt een bestaande bestelling bij in de database en slaat de wijzigingen op.
        /// </summary>
        /// <param name="order">Het Order object waarvan de wijzigingen moeten worden opgeslagen.</param>
        public async Task UpdateOrder(Order order) // AANGEPAST: Methode is nu async Task
        {
            _context.Entry(order).State = EntityState.Modified; // Markeer de entiteit als gewijzigd
            await _context.SaveChangesAsync(); // AANGEPAST: Gebruik SaveChangesAsync
        }
    }
}

