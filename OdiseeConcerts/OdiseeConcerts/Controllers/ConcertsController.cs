using Microsoft.AspNetCore.Mvc;
using OdiseeConcerts.Interfaces;
using OdiseeConcerts.ViewModels;
using Microsoft.AspNetCore.Authorization; // Nodig voor [Authorize]
using System.Security.Claims; // Nodig voor User.FindFirstValue(ClaimTypes.NameIdentifier)
using Microsoft.AspNetCore.Identity; // Nodig voor UserManager
using OdiseeConcerts.Models; // Nodig voor CustomUser, Concert
using System.Threading.Tasks; // Nodig voor async Task
using Microsoft.EntityFrameworkCore; // Nodig voor DbUpdateConcurrencyException, AnyAsync, FindAsync
using System; // Nodig voor DateTime.MinValue

namespace OdiseeConcerts.Controllers
{
    public class ConcertsController : Controller
    {
        private readonly IConcertService _concertService;
        private readonly ITicketOfferService _ticketOfferService;
        private readonly IOrderService _orderService;
        private readonly UserManager<CustomUser> _userManager;
        private readonly Data.ApplicationDbContext _context; // BEHOUDEN: Voor admin acties die de context direct gebruiken

        /// <summary>
        /// Constructor voor ConcertsController.
        /// Injecteert de benodigde services en ApplicationDbContext.
        /// </summary>
        public ConcertsController(IConcertService concertService,
                                  ITicketOfferService ticketOfferService,
                                  IOrderService orderService,
                                  UserManager<CustomUser> userManager,
                                  Data.ApplicationDbContext context) // Injecteer ApplicationDbContext
        {
            _concertService = concertService;
            _ticketOfferService = ticketOfferService;
            _orderService = orderService;
            _userManager = userManager;
            _context = context; // Toewijzen van de context
        }

        /// <summary>
        /// Toont een overzicht van alle concerten.
        /// </summary>
        public IActionResult Index()
        {
            var concerts = _concertService.GetAllConcerts();
            return View(concerts);
        }

        /// <summary>
        /// Toont de details van een specifiek concert, inclusief de beschikbare ticketaanbiedingen. (Publiek)
        /// </summary>
        /// <param name="id">Het ID van het concert.</param>
        /// <returns>De ConcertDetailsWithOffers View of NotFound.</returns>
        public IActionResult Details(int id)
        {
            var model = _concertService.GetConcertDetailsWithOffers(id);

            if (model == null)
            {
                return NotFound(); // Concert niet gevonden
            }

            return View(model);
        }


        // =============================================================
        // START CODE VOOR TICKETVERKOOP (BUY-pagina)
        // =============================================================

        /// <summary>
        /// Toont het formulier om tickets te kopen voor een specifieke TicketOffer. (GET)
        /// Vereist dat de gebruiker ingelogd is.
        /// </summary>
        /// <param name="id">Het ID van de TicketOffer.</param>
        /// <returns>De Buy View met het OrderFormViewModel, of NotFound als de TicketOffer niet bestaat.</returns>
        [Authorize] // Alleen ingelogde gebruikers kunnen tickets kopen
        public async Task<IActionResult> Buy(int id)
        {
            // Haal de ID van de ingelogde gebruiker op
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // Dit zou niet mogen gebeuren met [Authorize], maar voor de zekerheid.
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Haal de CustomUser op om te controleren op ledenkaart
            var user = await _userManager.FindByIdAsync(userId);
            bool hasMemberCard = user?.HasMemberCard ?? false; // Controleer de HasMemberCard property

            var model = _ticketOfferService.GetTicketOfferForOrder(id, hasMemberCard);

            if (model == null)
            {
                return NotFound(); // TicketOffer niet gevonden
            }

            // Stel de UserId in het model in, deze is nodig voor het aanmaken van de Order
            model.UserId = userId;

            return View(model);
        }

        /// <summary>
        /// Verwerkt het ingediende formulier voor het kopen van tickets. (POST)
        /// </summary>
        /// <param name="model">De OrderFormViewModel met de ingevoerde gegevens.</param>
        /// <returns>Redirect naar de Success pagina bij succes, of de Buy View met validatiefouten.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken] // Beveiliging tegen Cross-Site Request Forgery
        [Authorize]
        public async Task<IActionResult> Buy(OrderFormViewModel model)
        {
            // Haal de ID van de ingelogde gebruiker op
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || model.UserId != userId)
            {
                // Gebruiker is niet ingelogd of UserId in model klopt niet met ingelogde gebruiker.
                // Dit kan een poging tot manipulatie zijn.
                return Unauthorized();
            }

            // Voeg de UserId uit de claims toe aan het model, deze is cruciaal voor de service
            model.UserId = userId;

            // Haal de CustomUser opnieuw op om te controleren op ledenkaart, voor validatie
            var user = await _userManager.FindByIdAsync(userId);
            model.HasMemberCard = user?.HasMemberCard ?? false;

            // Als het model niet geldig is (bijv. NumberOfTickets buiten bereik),
            // of als de OrderService 0 retourneert (bestelling mislukt),
            // moeten we de view opnieuw laden met de juiste gegevens en foutmeldingen.
            if (!ModelState.IsValid)
            {
                // Herlaad de volledige OrderFormViewModel om alle display-eigenschappen opnieuw te vullen
                var currentTicketOffer = _ticketOfferService.GetTicketOfferForOrder(model.TicketOfferId, model.HasMemberCard);
                if (currentTicketOffer != null)
                {
                    model.Artist = currentTicketOffer.Artist;
                    model.Location = currentTicketOffer.Location;
                    model.Date = currentTicketOffer.Date;
                    model.TicketDescription = currentTicketOffer.TicketDescription;
                    model.PricePerTicket = currentTicketOffer.PricePerTicket;
                    model.TotalPrice = currentTicketOffer.TotalPrice; // Deze wordt door de service berekend
                    model.AvailableTicketsInOffer = currentTicketOffer.AvailableTicketsInOffer;
                }
                return View(model);
            }

            // Controleer of de voorraad voldoende is voordat de bestelling wordt gecreëerd.
            // Dit is een dubbele controle naast de service, voor een betere UX.
            // De service zal ook valideren, maar hier kunnen we al een ModelState error toevoegen.
            var preCheckTicketOffer = _ticketOfferService.GetTicketOfferForOrder(model.TicketOfferId, model.HasMemberCard);
            if (preCheckTicketOffer == null || preCheckTicketOffer.AvailableTicketsInOffer < model.NumberOfTickets)
            {
                ModelState.AddModelError(string.Empty, "Niet voldoende tickets beschikbaar voor dit type.");
                // Herlaad modelgegevens voor weergave
                if (preCheckTicketOffer != null)
                {
                    model.Artist = preCheckTicketOffer.Artist;
                    model.Location = preCheckTicketOffer.Location;
                    model.Date = preCheckTicketOffer.Date;
                    model.TicketDescription = preCheckTicketOffer.TicketDescription;
                    model.PricePerTicket = preCheckTicketOffer.PricePerTicket;
                    model.TotalPrice = preCheckTicketOffer.TotalPrice; // Deze wordt door de service berekend
                    model.AvailableTicketsInOffer = preCheckTicketOffer.AvailableTicketsInOffer;
                }
                return View(model);
            }

            // Probeer de bestelling te creëren via de OrderService
            int orderId = await _orderService.CreateOrder(model);

            if (orderId > 0)
            {
                // Bestelling succesvol geplaatst, redirect naar de succes-pagina
                return RedirectToAction("Success", "Orders", new { id = orderId });
            }
            else
            {
                // Er is iets misgegaan bij het aanmaken van de bestelling (bijv. voorraadprobleem, prijsmismatch in service)
                ModelState.AddModelError(string.Empty, "Er is een probleem opgetreden bij het plaatsen van uw bestelling. Probeer het opnieuw.");
                // Herlaad de volledige OrderFormViewModel om alle display-eigenschappen opnieuw te vullen
                var failedTicketOffer = _ticketOfferService.GetTicketOfferForOrder(model.TicketOfferId, model.HasMemberCard);
                if (failedTicketOffer != null)
                {
                    model.Artist = failedTicketOffer.Artist;
                    model.Location = failedTicketOffer.Location;
                    model.Date = failedTicketOffer.Date;
                    model.TicketDescription = failedTicketOffer.TicketDescription;
                    model.PricePerTicket = failedTicketOffer.PricePerTicket;
                    model.TotalPrice = failedTicketOffer.TotalPrice; // Deze wordt door de service berekend
                    model.AvailableTicketsInOffer = failedTicketOffer.AvailableTicketsInOffer;
                }
                return View(model);
            }
        }
        // =============================================================
        // EINDE CODE VOOR TICKETVERKOOP (BUY-pagina)
        // =============================================================


        // =============================================================
        // START ADMIN CRUD ACTIES (behouden zoals in jouw vorige versie)
        // =============================================================

        // GET: Concerts/Create (Admin functionaliteit)
        [Authorize(Policy = "IsAdmin")] // Specifieke autorisatie voor admin-acties
        public IActionResult Create()
        {
            return View();
        }

        // POST: Concerts/Create (Admin functionaliteit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "IsAdmin")] // Specifieke autorisatie voor admin-acties
        public async Task<IActionResult> Create([Bind("Artist,Location,Date,Id")] Concert concert)
        {
            if (ModelState.IsValid)
            {
                _context.Add(concert);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(concert);
        }

        // GET: Concerts/Edit/5 (Admin functionaliteit)
        [Authorize(Policy = "IsAdmin")] // Specifieke autorisatie voor admin-acties
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var concert = await _context.Concerts.FindAsync(id);
            if (concert == null)
            {
                return NotFound();
            }
            return View(concert);
        }

        // POST: Concerts/Edit/5 (Admin functionaliteit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "IsAdmin")] // Specifieke autorisatie voor admin-acties
        public async Task<IActionResult> Edit(int id, [Bind("Artist,Location,Date,Id")] Concert concert)
        {
            if (id != concert.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(concert);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConcertExists(concert.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(concert);
        }

        // GET: Concerts/Delete/5 (Admin functionaliteit)
        [Authorize(Policy = "IsAdmin")] // Specifieke autorisatie voor admin-acties
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var concert = await _context.Concerts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (concert == null)
            {
                return NotFound();
            }

            return View(concert);
        }

        // POST: Concerts/Delete/5 (Admin functionaliteit)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "IsAdmin")] // Specifieke autorisatie voor admin-acties
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var concert = await _context.Concerts.FindAsync(id);
            if (concert != null)
            {
                _context.Concerts.Remove(concert);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConcertExists(int id)
        {
            return _context.Concerts.Any(e => e.Id == id);
        }
    }
}
