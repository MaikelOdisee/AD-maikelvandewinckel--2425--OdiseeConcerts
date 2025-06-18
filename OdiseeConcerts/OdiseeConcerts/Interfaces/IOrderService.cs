using System.Collections.Generic; // Nodig voor IEnumerable
using OdiseeConcerts.ViewModels; // Nodig voor OrderFormViewModel en OrderViewModel

namespace OdiseeConcerts.Interfaces
{
    // Interface voor de Order Service.
    // Definieert de methoden die een Order Service moet implementeren.
    public interface IOrderService
    {
        /// <summary>
        /// Creëert een nieuwe bestelling op basis van de gegevens in de OrderFormViewModel.
        /// </summary>
        /// <param name="model">De OrderFormViewModel met de bestelgegevens.</param>
        /// <returns>Het ID van de zojuist aangemaakte bestelling.</returns>
        Task<int> CreateOrder(OrderFormViewModel model); // AANGEPAST: Methode is nu async Task<int>

        /// <summary>
        /// Haalt een specifieke bestelling op en mapt deze naar een OrderViewModel.
        /// </summary>
        /// <param name="orderId">Het ID van de bestelling.</param>
        /// <returns>Een OrderViewModel met de details van de bestelling, of null als niet gevonden.</returns>
        OrderViewModel? GetOrderById(int orderId);

        /// <summary>
        /// Werkt de betaalstatus van een bestelling bij.
        /// </summary>
        /// <param name="orderId">Het ID van de bestelling.</param>
        /// <param name="paid">De nieuwe betaalstatus (true voor betaald, false voor onbetaald).</param>
        void UpdatePaid(int orderId, bool paid);

        // Let op: GetOrdersByStatus is hier ook al genoemd in je architectuur,
        // maar we implementeren hem pas als we die functionaliteit echt nodig hebben.
        // IEnumerable<OrderViewModel> GetOrdersByStatus(bool paid);
    }
}
