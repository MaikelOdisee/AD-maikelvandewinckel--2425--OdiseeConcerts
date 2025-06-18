using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OdiseeConcerts.Data;
using OdiseeConcerts.Models;
using Microsoft.AspNetCore.Authorization; // Nodig voor [Authorize]

namespace OdiseeConcerts.Controllers
{
    [Authorize(Policy = "IsAdmin")] // Beveiligt deze controller voor admins
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Orders
                                            .Include(o => o.TicketOffer)
                                                .ThenInclude(to => to.Concert)
                                            .Include(o => o.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Orders/Details/5
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

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["TicketOfferId"] = new SelectList(_context.TicketOffers.Include(to => to.Concert), "Id", "TicketType");

            var users = _context.Users
                                .AsEnumerable()
                                .Select(u => new
                                {
                                    u.Id,
                                    DisplayName = $"{u.Email} ({u.FirstName} {u.LastName})"
                                })
                                .OrderBy(u => u.DisplayName)
                                .ToList();
            ViewData["UserId"] = new SelectList(users, "Id", "DisplayName");

            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,NumTickets,TotalPrice,Paid,DiscountApplied,TicketOfferId,Id")] Order order)
        {
            // Omdat UserName niet langer in het model zit, is er geen logica meer nodig voor UserName.
            // De validatie van UserId wordt nu volledig afgehandeld door [Required] op UserId in het model.

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(order);
                    await _context.SaveChangesAsync(); // <-- Hier kan de fout optreden
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    // Vang database-gerelateerde fouten op
                    ModelState.AddModelError("", "Er is een fout opgetreden bij het opslaan van de bestelling. Controleer de invoer en probeer opnieuw.");
                    // Optioneel: Voeg meer details toe aan de Modelstate of log de fout
                    // Je kunt de volledige foutdetails loggen naar console of een logbestand:
                    Console.WriteLine($"DbUpdateException tijdens opslaan van Order: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                        ModelState.AddModelError("", $"Gedetailleerde fout: {ex.InnerException.Message}");
                    }
                    // Als er een specifiek veld de fout veroorzaakt (bijv. NULL constraint)
                    // ModelState.AddModelError("FieldName", "Foutmelding voor dat veld.");
                }
                catch (Exception ex)
                {
                    // Vang andere algemene fouten op
                    ModelState.AddModelError("", $"Er is een onverwachte fout opgetreden: {ex.Message}");
                    Console.WriteLine($"Onverwachte fout tijdens opslaan van Order: {ex.Message}");
                }
            }

            // Herlaad dropdowns als ModelState niet geldig is, met de eerder geselecteerde waarden
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

        // GET: Orders/Edit/5
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

        // POST: Orders/Edit/5
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
                    await _context.SaveChangesAsync(); // <-- Hier kan de fout optreden
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Dit is de specifieke catch voor concurrency, behouden
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
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
                return RedirectToAction(nameof(Index)); // Redirect alleen bij succesvol opslaan
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

        // GET: Orders/Delete/5
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

        // POST: Orders/Delete/5
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

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}

