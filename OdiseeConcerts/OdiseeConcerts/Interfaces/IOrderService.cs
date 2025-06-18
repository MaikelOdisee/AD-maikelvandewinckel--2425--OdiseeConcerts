using System.Collections.Generic; // Nodig voor IEnumerable
using System.Threading.Tasks; // Nodig voor Task
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
        /// <returns>Het ID van de zojuist aangemaakte bestelling (of 0 bij falen).</returns>
        Task<int> CreateOrder(OrderFormViewModel model); // Zorg dat de parameter hier OrderFormViewModel model is

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
        /// <returns>True als de update succesvol was, anders False.</returns>
        Task<bool> UpdatePaid(int orderId, bool paid); // AANGEPAST: Return type is nu Task<bool>

        /// <summary>
        /// Bevestigt de betaling voor een specifieke bestelling.
        /// </summary>
        /// <param name="orderId">Het ID van de bestelling waarvan de betaling moet worden bevestigd.</param>
        /// <returns>True als de betaling succesvol is bevestigd, anders False.</returns>
        Task<bool> ConfirmOrderPaymentAsync(int orderId);
    }
}
