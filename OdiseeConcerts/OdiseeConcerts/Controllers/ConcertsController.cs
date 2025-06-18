using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OdiseeConcerts.Data;
using OdiseeConcerts.Models;
using OdiseeConcerts.Interfaces; // TOEGEVOEGD: Nodig voor IConcertService
using OdiseeConcerts.ViewModels; // TOEGEVOEGD: Nodig voor ConcertViewModel
using Microsoft.AspNetCore.Authorization; // TOEGEVOEGD: Nodig voor [Authorize]

namespace OdiseeConcerts.Controllers
{
    // De ConcertsController is nu verantwoordelijk voor de gebruikersgerichte weergave van concerten.
    // De admin-gerelateerde acties (Create, Edit, Delete) hebben nu specifieke autorisatie.
    // LET OP: De algemene [Authorize(Policy = "IsAdmin")] op de controller is VERWIJDERD,
    // omdat de Index-pagina voor alle gebruikers (ook niet-ingelogd) zichtbaar moet zijn.
    public class ConcertsController : Controller
    {
        private readonly IConcertService _concertService; // AANGEPAST: Injecteer IConcertService
        private readonly ApplicationDbContext _context; // BEHOUDEN: Voor admin acties die de context direct gebruiken

        // AANGEPAST: Constructor om IConcertService te injecteren
        public ConcertsController(IConcertService concertService, ApplicationDbContext context)
        {
            _concertService = concertService;
            _context = context; // Behoud de context voor admin acties
        }

        // GET: Concerts (de startpagina voor de gebruiker)
        // Deze actie haalt alle concerten op via de service en presenteert ze.
        public IActionResult Index()
        {
            // Gebruik de IConcertService om alle concerten op te halen als ConcertViewModel.
            var concerts = _concertService.GetAllConcerts();
            return View(concerts);
        }

        // De onderstaande acties (Details, Create, Edit, Delete) blijven voorlopig in deze controller.
        // Ze gebruiken nog steeds de directe _context injectie, wat prima is voor admin functionaliteit.
        // Als je later een aparte admin-controller of admin-gebied maakt, kun je deze verplaatsen.

        // GET: Concerts/Details/5 (Admin functionaliteit - kan publiek zijn, maar als het admin details zijn, autoriseren)
        // Voor nu laten we deze even openbaar, tenzij het echte admin details zijn.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Gebruik de context direct voor details, of implementeer een service-methode hiervoor indien gewenst.
            var concert = await _context.Concerts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (concert == null)
            {
                return NotFound();
            }

            return View(concert);
        }

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
