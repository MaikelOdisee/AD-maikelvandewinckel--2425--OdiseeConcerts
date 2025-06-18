using OdiseeConcerts.Models; // Nodig voor TicketOffer model
using System.Linq; // Nodig voor .Sum()
using System; // Nodig voor DateTime
// using System.IO; // Niet langer direct nodig voor Path.Combine in deze context

namespace OdiseeConcerts.ViewModels
{
    /// <summary>
    /// ViewModel voor het weergeven van concertinformatie op de Index-pagina.
    /// Bevat alleen de benodigde eigenschappen voor de weergave en logica voor de knopstatus.
    /// </summary>
    public class ConcertViewModel
    {
        public int Id { get; set; }
        public string Artist { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        // De lijst van TicketOffers die bij dit concert horen.
        // Nodig om het totale aantal beschikbare tickets te berekenen.
        public IEnumerable<TicketOffer>? TicketOffers { get; set; } // Kan null zijn als er geen aanbiedingen zijn

        /// <summary>
        /// Deze property genereert de URL van de artiestenafbeelding.
        /// De bestandsnamen moeten overeenkomen met de geformatteerde artiestennaam + .png.
        /// Bijvoorbeeld: "Taylor Swift" wordt "/img/taylorswift.png".
        /// </summary>
        public string ArtistPicture
        {
            get
            {
                // Formatteer de artiestennaam voor de bestandsnaam (bijv. "Taylor Swift" wordt "taylorswift")
                var formattedArtistName = Artist.Replace(" ", "").ToLower();

                // Constructeer het pad naar de afbeelding in wwwroot/img met de .png extensie.
                // Dit pad wordt direct gebruikt in de <img> tag.
                return $"/img/{formattedArtistName}.png";
            }
        }

        /// <summary>
        /// Berekent het totale aantal beschikbare tickets voor dit concert.
        /// Dit is de som van NumTickets van alle gerelateerde TicketOffers.
        /// </summary>
        public int AvailableTickets => TicketOffers?.Sum(to => to.NumTickets) ?? 0;

        /// <summary>
        /// Bepaalt de tekst die op de 'Koop tickets' knop moet verschijnen,
        /// afhankelijk van het aantal beschikbare tickets.
        /// </summary>
        public string ButtonText
        {
            get
            {
                if (AvailableTickets > 500)
                {
                    return "Koop tickets";
                }
                else if (AvailableTickets > 0) // Tussen 1 en 500
                {
                    return "Laatste tickets!";
                }
                else // 0 beschikbare tickets
                {
                    return "Uitverkocht";
                }
            }
        }

        /// <summary>
        /// Bepaalt de Bootstrap CSS-klasse voor de 'Koop tickets' knop,
        /// afhankelijk van het aantal beschikbare tickets.
        /// </summary>
        public string ButtonClass
        {
            get
            {
                if (AvailableTickets > 500)
                {
                    return "btn-primary"; // Standaard blauw
                }
                else if (AvailableTickets > 0)
                {
                    return "btn-warning"; // Oranje/geel voor waarschuwing
                }
                else
                {
                    return "btn-danger"; // Rood voor uitverkocht
                }
            }
        }
    }
}

