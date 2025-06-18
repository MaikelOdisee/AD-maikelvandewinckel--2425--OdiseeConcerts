using System.Collections.Generic; // Nodig voor IEnumerable, al is het nu nog niet direct gebruikt
using OdiseeConcerts.ViewModels; // Nodig voor OrderFormViewModel en TicketOfferUpdateViewModel (later te maken)
using System.Threading.Tasks; // NIEUW: Nodig voor Task

namespace OdiseeConcerts.Interfaces
{
    // Interface voor de TicketOffer Service.
    // Definieert de methoden die een TicketOffer Service moet implementeren.
    public interface ITicketOfferService
    {
        /// <summary>
        /// Haalt een TicketOffer op en mapt deze naar een OrderFormViewModel voor de bestelpagina.
        /// Past eventueel korting toe op basis van lidmaatschap.
        /// </summary>
        /// <param name="id">Het ID van de TicketOffer.</param>
        /// <param name="hasMemberCard">Geeft aan of de gebruiker een ledenkaart heeft.</param>
        /// <returns>Een OrderFormViewModel met de details van de ticketaanbieding en prijs.</returns>
        OrderFormViewModel GetTicketOfferForOrder(int id, bool hasMemberCard);

        /// <summary>
        /// Werkt het aantal beschikbare tickets van een TicketOffer bij.
        /// </summary>
        /// <param name="model">Een ViewModel met de bijgewerkte informatie over de ticketaanbieding.</param>
        Task UpdateTicketOffer(TicketOfferUpdateViewModel model); // AANGEPAST: Retourtype is nu Task
    }
}
