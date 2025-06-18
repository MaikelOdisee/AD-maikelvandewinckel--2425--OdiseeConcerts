using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OdiseeConcerts.Data;
using OdiseeConcerts.Models;
using Microsoft.AspNetCore.Authorization; // Nodig voor [Authorize] en [AllowAnonymous]
using OdiseeConcerts.Interfaces; // Nodig voor IOrderService
using OdiseeConcerts.ViewModels; // Nodig voor OrderViewModel

namespace OdiseeConcerts.Controllers
{
    // De OrdersController is verantwoordelijk voor alle bestellingen gerelateerde functionaliteit.
    // De admin-acties (Index, Details, Edit, Delete) zijn beveiligd met een policy.
    // De Success-actie is openbaar toegankelijk voor gebruikers na een bestelling.
    [Authorize(Policy = "IsAdmin")] // Standaardbeveiliging voor de meeste acties in deze controller
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context; // Directe toegang tot de database context voor admin CRUD
        private readonly IOrderService _orderService; // Service voor bestelgerelateerde bedrijfslogica

        /// <summary>
        /// Constructor voor OrdersController.
        /// Injecteert de ApplicationDbContext en de IOrderService.
        /// </summary>
        public OrdersController(ApplicationDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        /// <summary>
        /// Toont een overzicht van alle bestellingen. (Admin functionaliteit)
        /// Laadt gerelateerde TicketOffer, Concert en User gegevens.
        /// </summary>
        // Deze actie is beveiligd door de [Authorize(Policy = "IsAdmin")] attribute op de controller.
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Orders
                                               .Include(o => o.TicketOffer)
                                                   .ThenInclude(to => to.Concert) // Laad Concert via TicketOffer
                                               .Include(o => o.User); // Laad de gerelateerde CustomUser
            return View(await applicationDbContext.ToListAsync());
        }

        /// <summary>
        /// NIEUW: Toont een overzicht van alle ONBETAALDE bestellingen. (Admin functionaliteit)
        /// </summary>
        // Deze actie is beveiligd door de [Authorize(Policy = "IsAdmin")] attribute op de controller.
        public async Task<IActionResult> UnpaidOrders()
        {
            var unpaidOrders = await _context.Orders
                                            .Where(o => !o.Paid) // Filter op onbetaalde bestellingen
                                            .Include(o => o.TicketOffer)
                                                .ThenInclude(to => to.Concert)
                                            .Include(o => o.User)
                                            .ToListAsync();
            // Je kunt hier ook een aparte ViewModel maken als de weergegeven data afwijkt van de Order klasse.
            return View(unpaidOrders);
        }

        /// <summary>
        /// Toont de details van een specifieke bestelling. (Admin functionaliteit)
        /// Laadt gerelateerde TicketOffer, Concert en User gegevens.
        /// </summary>
        /// <param name="id">Het ID van de bestelling.</param>
        // Deze actie is beveiligd door de [Authorize(Policy = "IsAdmin")] attribute op de controller.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.TicketOffer)
                    .ThenInclude(to => to.Concert)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // ==============================================================
        // VERWIJDERD: GET en POST Create acties voor Orders.
        // Deze functionaliteit is niet langer gewenst in het admin-paneel.
        // ==============================================================
        // Voorheen (ter referentie, deze code is nu verwijderd):
        // /// <summary>
        // /// Toont het formulier voor het aanmaken van een nieuwe bestelling. (Admin functionaliteit)
        // /// Vult ViewData met SelectLists voor TicketOffers en Users.
        // /// </summary>
        // public IActionResult Create()
        // {
        //     ViewData["TicketOfferId"] = new SelectList(_context.TicketOffers.Include(to => to.Concert), "Id", "TicketType");
        //     var users = _context.Users.AsEnumerable().Select(u => new { u.Id, DisplayName = $"{u.Email} ({u.FirstName} {u.LastName})" }).OrderBy(u => u.DisplayName).ToList();
        //     ViewData["UserId"] = new SelectList(users, "Id", "DisplayName");
        //     return View();
        // }

        // /// <summary>
        // /// Verwerkt het ingediende formulier voor het aanmaken van een bestelling. (Admin functionaliteit)
        // /// </summary>
        // /// <param name="order">Het Order model dat wordt gebonden vanuit het formulier.</param>
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create([Bind("UserId,NumTickets,TotalPrice,Paid,DiscountApplied,TicketOfferId,Id")] Order order)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         try
        //         {
        //             _context.Add(order);
        //             await _context.SaveChangesAsync();
        //             return RedirectToAction(nameof(Index));
        //         }
        //         catch (DbUpdateException ex)
        //         {
        //             ModelState.AddModelError("", "Er is een fout opgetreden bij het opslaan van de bestelling. Controleer de invoer en probeer opnieuw.");
        //             Console.WriteLine($"DbUpdateException tijdens opslaan van Order: {ex.Message}");
        //             if (ex.InnerException != null)
        //             {
        //                 Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        //                 ModelState.AddModelError("", $"Gedetailleerde fout: {ex.InnerException.Message}");
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             ModelState.AddModelError("", $"Er is een onverwachte fout opgetreden: {ex.Message}");
        //             Console.WriteLine($"Onverwachte fout tijdens opslaan van Order: {ex.Message}");
        //         }
        //     }
        //     ViewData["TicketOfferId"] = new SelectList(_context.TicketOffers.Include(to => to.Concert), "Id", "TicketType", order.TicketOfferId);
        //     var usersOnError = _context.Users.AsEnumerable().Select(u => new { u.Id, DisplayName = $"{u.Email} ({u.FirstName} {u.LastName})" }).OrderBy(u => u.DisplayName).ToList();
        //     ViewData["UserId"] = new SelectList(usersOnError, "Id", "DisplayName", order.UserId);
        //     return View(order);
        // }
        // ==============================================================


        /// <summary>
        /// Toont het formulier voor het bewerken van een bestaande bestelling. (Admin functionaliteit)
        /// </summary>
        /// <param name="id">Het ID van de te bewerken bestelling.</param>
        // Deze actie is beveiligd door de [Authorize(Policy = "IsAdmin")] attribute op de controller.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            // Vult de dropdowns met TicketOffers (met Concert info) en Users.
            ViewData["TicketOfferId"] = new SelectList(_context.TicketOffers.Include(to => to.Concert), "Id", "TicketType", order.TicketOfferId);
            var users = _context.Users
                                .AsEnumerable()
                                .Select(u => new
                                {
                                    u.Id,
                                    DisplayName = $"{u.Email} ({u.FirstName} {u.LastName})"
                                })
                                .OrderBy(u => u.DisplayName)
                                .ToList();
            ViewData["UserId"] = new SelectList(users, "Id", "DisplayName", order.UserId);

            return View(order);
        }

        /// <summary>
        /// Verwerkt het ingediende formulier voor het bewerken van een bestelling. (Admin functionaliteit)
        /// </summary>
        /// <param name="id">Het ID van de te bewerken bestelling.</param>
        /// <param name="order">Het Order model dat wordt gebonden vanuit het formulier.</param>
        // Deze actie is beveiligd door de [Authorize(Policy = "IsAdmin")] attribute op de controller.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,NumTickets,TotalPrice,Paid,DiscountApplied,TicketOfferId,Id")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index)); // Redirect alleen bij succesvol opslaan
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Specifieke catch voor concurrency, behouden
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Gooi de fout opnieuw als het geen NotFound is
                    }
                }
                catch (DbUpdateException ex)
                {
                    // Vang algemene DbUpdateExceptions op
                    ModelState.AddModelError("", "Er is een fout opgetreden bij het opslaan van de wijzigingen. Controleer de invoer en probeer opnieuw.");
                    Console.WriteLine($"DbUpdateException tijdens bijwerken van Order: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                        ModelState.AddModelError("", $"Gedetailleerde fout: {ex.InnerException.Message}");
                    }
                }
                catch (Exception ex)
                {
                    // Vang andere algemene fouten op
                    ModelState.AddModelError("", $"Er is een onverwachte fout opgetreden: {ex.Message}");
                    Console.WriteLine($"Onverwachte fout tijdens bijwerken van Order: {ex.Message}");
                }
            }
            // Herlaad dropdowns als ModelState niet geldig is
            ViewData["TicketOfferId"] = new SelectList(_context.TicketOffers.Include(to => to.Concert), "Id", "TicketType", order.TicketOfferId);
            var usersOnError = _context.Users
                                        .AsEnumerable()
                                        .Select(u => new
                                        {
                                            u.Id,
                                            DisplayName = $"{u.Email} ({u.FirstName} {u.LastName})"
                                        })
                                        .OrderBy(u => u.DisplayName)
                                        .ToList();
            ViewData["UserId"] = new SelectList(usersOnError, "Id", "DisplayName", order.UserId);

            return View(order);
        }

        /// <summary>
        /// Toont een bevestigingspagina voor het verwijderen van een bestelling. (Admin functionaliteit)
        /// </summary>
        /// <param name="id">Het ID van de te verwijderen bestelling.</param>
        // Deze actie is beveiligd door de [Authorize(Policy = "IsAdmin")] attribute op de controller.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.TicketOffer)
                    .ThenInclude(to => to.Concert)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        /// <summary>
        /// Verwerkt het verwijderen van een bestelling na bevestiging. (Admin functionaliteit)
        /// </summary>
        /// <param name="id">Het ID van de te verwijderen bestelling.</param>
        // Deze actie is beveiligd door de [Authorize(Policy = "IsAdmin")] attribute op de controller.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// NIEUW: Bevestigt de betaling voor een specifieke bestelling via de OrderService. (Admin functionaliteit)
        /// </summary>
        /// <param name="id">Het ID van de bestelling waarvan de betaling moet worden bevestigd.</param>
        /// <returns>Redirect naar de UnpaidOrders lijst.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var success = await _orderService.ConfirmOrderPaymentAsync(id);

            if (!success)
            {
                // Optioneel: voeg een foutmelding toe aan TempData of ModelState
                TempData["ErrorMessage"] = "Kon de betaling niet bevestigen. Bestelling mogelijk niet gevonden of een fout opgetreden.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Betaling voor bestelling {id} succesvol bevestigd.";
            }

            return RedirectToAction(nameof(UnpaidOrders)); // Ga terug naar de lijst met onbetaalde bestellingen
        }

        /// <summary>
        /// Hulpfunctie om te controleren of een bestelling bestaat.
        /// </summary>
        /// <param name="id">Het ID van de bestelling.</param>
        /// <returns>True als de bestelling bestaat, anders False.</returns>
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        /// <summary>
        /// Toont een bevestigingspagina na een succesvolle bestelling. (Gebruikersgericht)
        /// Deze actie is expliciet toegestaan voor niet-ingelogde gebruikers.
        /// </summary>
        /// <param name="id">Het ID van de zojuist geplaatste bestelling.</param>
        /// <returns>De Success View met de besteldetails.</returns>
        [AllowAnonymous] // Iedereen mag deze pagina zien na een bestelling
        public IActionResult Success(int id)
        {
            var orderViewModel = _orderService.GetOrderById(id);

            if (orderViewModel == null)
            {
                return NotFound(); // Bestelling niet gevonden
            }

            return View(orderViewModel);
        }
    }
}
