using System.Collections.Generic; // Nodig voor IEnumerable
using OdiseeConcerts.Models; // Nodig voor Order model
using System.Threading.Tasks; // TOEGEVOEGD: Nodig voor Task

namespace OdiseeConcerts.Interfaces
{
    // Interface voor de Order Repository.
    // Definieert de methoden die een Order Repository moet implementeren.
    public interface IOrderRepository
    {
        /// <summary>
        /// Voegt een nieuwe bestelling toe aan de database.
        /// </summary>
        /// <param name="order">Het Order object dat moet worden toegevoegd.</param>
        Task AddOrder(Order order); // AANGEPAST: Methode is nu async Task

        /// <summary>
        /// Haalt een specifieke bestelling op basis van het ID.
        /// LET OP: Deze methode laadt GEEN gerelateerde entiteiten (zoals TicketOffer, Concert, User) in.
        /// </summary>
        /// <param name="id">Het ID van de bestelling.</param>
        /// <returns>Het Order object, of null als niet gevonden.</returns>
        Order? GetOrderById(int id);

        /// <summary>
        /// Haalt een specifieke bestelling op basis van het ID, inclusief gerelateerde TicketOffer, Concert en User.
        /// </summary>
        /// <param name="id">Het ID van de bestelling.</param>
        /// <returns>Het Order object met alle geladen details, of null als niet gevonden.</returns>
        Order? GetOrderWithDetailsById(int id);

        /// <summary>
        /// Werkt een bestaande bestelling bij in de database.
        /// </summary>
        /// <param name="order">Het Order object waarvan de wijzigingen moeten worden opgeslagen.</param>
        Task UpdateOrder(Order order); // AANGEPAST: Methode is nu async Task

        // Let op: GetOrdersByStatus is hier ook al genoemd in je architectuur,
        // maar we implementeren hem pas als we die functionaliteit echt nodig hebben.
        // IEnumerable<Order> GetOrdersByStatus(bool paid);
    }
}
