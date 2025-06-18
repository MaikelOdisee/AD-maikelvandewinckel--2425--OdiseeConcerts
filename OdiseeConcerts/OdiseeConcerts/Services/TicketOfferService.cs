using OdiseeConcerts.Interfaces;
using OdiseeConcerts.Models;
using OdiseeConcerts.ViewModels;
using System.Linq; // Nodig voor .FirstOrDefault()
using System; // Nodig voor DateTime
using System.Threading.Tasks; // Nodig voor Task (voor de UpdateTicketOffer methode)

namespace OdiseeConcerts.Services
{
    // Implementeert de ITicketOfferService interface.
    // Verantwoordelijk voor de bedrijfslogica gerelateerd aan TicketOffers.
    public class TicketOfferService : ITicketOfferService
    {
        private readonly ITicketOfferRepository _ticketOfferRepository;
        private const decimal MemberCardDiscountPercentage = 0.10m; // 10% korting voor leden

        /// <summary>
        /// Constructor voor TicketOfferService.
        /// Injecteert de ITicketOfferRepository om toegang te krijgen tot data.
        /// </summary>
        /// <param name="ticketOfferRepository">De repository voor ticketaanbiedingdata.</param>
        public TicketOfferService(ITicketOfferRepository ticketOfferRepository)
        {
            _ticketOfferRepository = ticketOfferRepository;
        }

        /// <summary>
        /// Haalt een TicketOffer op en mapt deze naar een OrderFormViewModel voor de bestelpagina.
        /// Past eventueel korting toe op basis van lidmaatschap.
        /// </summary>
        /// <param name="id">Het ID van de TicketOffer.</param>
        /// <param name="hasMemberCard">Geeft aan of de gebruiker een ledenkaart heeft.</param>
        /// <returns>Een OrderFormViewModel met de details van de ticketaanbieding en prijs, of null als de ticketaanbieding niet bestaat.</returns>
        public OrderFormViewModel GetTicketOfferForOrder(int id, bool hasMemberCard)
        {
            // Gebruik de nieuwe repository methode om TicketOffer EN Concert op te halen.
            var ticketOffer = _ticketOfferRepository.GetTicketOfferWithConcertById(id);

            if (ticketOffer == null)
            {
                return null; // Geen ticketaanbieding gevonden
            }

            // Controleer of het gerelateerde Concert geladen is (zou moeten zijn met Include)
            if (ticketOffer.Concert == null)
            {
                // Dit zou niet moeten gebeuren als Include correct werkt, maar is een veiligheidscheck.
                // Je kunt hier loggen of een uitzondering gooien.
                // Voor nu, retourneren we null, wat aangeeft dat er een probleem is met de data.
                return null;
            }

            decimal finalPrice = ticketOffer.Price;
            bool discountAppliedForDisplay = false; // Deze variabele is hier niet direct nodig voor de ViewModel, maar voor context

            if (hasMemberCard)
            {
                finalPrice -= (ticketOffer.Price * MemberCardDiscountPercentage);
                discountAppliedForDisplay = true;
            }

            // Genereer de afbeeldings-URL op dezelfde manier als in ConcertViewModel
            // Zorg ervoor dat je afbeeldingsbestanden (bijv. .png) in de wwwroot/img map staan
            // en overeenkomen met de geformatteerde artiestennamen (bijv. "Taylor Swift" -> "taylorswift.png").
            var formattedArtistName = ticketOffer.Concert.Artist.Replace(" ", "").ToLower();
            var artistPicturePath = $"/img/{formattedArtistName}.png";


            return new OrderFormViewModel
            {
                ConcertId = ticketOffer.Concert.Id,
                Artist = ticketOffer.Concert.Artist,
                Location = ticketOffer.Concert.Location,
                Date = ticketOffer.Concert.Date,
                ArtistPicture = artistPicturePath, // TOEGEVOEGD: ArtistPicture nu gevuld
                TicketOfferId = ticketOffer.Id,
                TicketDescription = ticketOffer.TicketType,
                PricePerTicket = finalPrice,
                NumberOfTickets = 1, // Standaard 1 ticket voor de initiële weergave
                HasMemberCard = hasMemberCard,
                TotalPrice = finalPrice, // Bij initiële weergave is Totaalprijs = Prijs per ticket * 1
                AvailableTicketsInOffer = ticketOffer.NumTickets,
            };
        }

        /// <summary>
        /// Werkt het aantal beschikbare tickets van een TicketOffer bij.
        /// </summary>
        /// <param name="model">Een ViewModel met de bijgewerkte informatie over de ticketaanbieding.</param>
        public async Task UpdateTicketOffer(TicketOfferUpdateViewModel model) // AANGEPAST: Methode is nu async Task
        {
            var ticketOffer = _ticketOfferRepository.GetTicketOfferById(model.Id); // Gebruik de methode zonder Include, want we updaten alleen de TicketOffer zelf
            if (ticketOffer != null)
            {
                ticketOffer.NumTickets = model.NewNumTickets;
                await _ticketOfferRepository.UpdateTicketOffer(ticketOffer); // Roep de async Update methode van de repository aan
            }
            else
            {
                // Loggen of een uitzondering gooien als de ticketOffer niet gevonden wordt.
                // Voor dit voorbeeld laten we het stil, maar in een echte app is logging essentieel.
                Console.WriteLine($"Waarschuwing: TicketOffer met ID {model.Id} niet gevonden voor update.");
            }
        }
    }
}

