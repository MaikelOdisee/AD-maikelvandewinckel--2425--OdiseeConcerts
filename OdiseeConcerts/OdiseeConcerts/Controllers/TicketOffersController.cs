using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OdiseeConcerts.Data;
using OdiseeConcerts.Models;
using Microsoft.AspNetCore.Authorization; // TOEGEVOEGD: Nodig voor [Authorize]

namespace OdiseeConcerts.Controllers
{
    [Authorize(Policy = "IsAdmin")] // TOEGEVOEGD: Beveiligt deze controller voor admins
    public class TicketOffersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketOffersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TicketOffers
        public async Task<IActionResult> Index()
        {
            // Laad ook de gerelateerde Concert entiteit voor weergave
            var applicationDbContext = _context.TicketOffers.Include(t => t.Concert);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TicketOffers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketOffer = await _context.TicketOffers
                .Include(t => t.Concert) // Includen van Concert is nodig voor details
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketOffer == null)
            {
                return NotFound();
            }

            return View(ticketOffer);
        }

        // GET: TicketOffers/Create
        public IActionResult Create()
        {
            ViewData["ConcertId"] = new SelectList(_context.Concerts, "Id", "Artist"); // Geef artiest weer in dropdown
            return View();
        }

        // POST: TicketOffers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to. For
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketType,NumTickets,Price,ConcertId,Id")] TicketOffer ticketOffer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticketOffer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConcertId"] = new SelectList(_context.Concerts, "Id", "Artist", ticketOffer.ConcertId);
            return View(ticketOffer);
        }

        // GET: TicketOffers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketOffer = await _context.TicketOffers.FindAsync(id);
            if (ticketOffer == null)
            {
                return NotFound();
            }
            ViewData["ConcertId"] = new SelectList(_context.Concerts, "Id", "Artist", ticketOffer.ConcertId);
            return View(ticketOffer);
        }

        // POST: TicketOffers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to. For
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketType,NumTickets,Price,ConcertId,Id")] TicketOffer ticketOffer)
        {
            if (id != ticketOffer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketOffer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketOfferExists(ticketOffer.Id))
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
            ViewData["ConcertId"] = new SelectList(_context.Concerts, "Id", "Artist", ticketOffer.ConcertId);
            return View(ticketOffer);
        }

        // GET: TicketOffers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketOffer = await _context.TicketOffers
                .Include(t => t.Concert)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketOffer == null)
            {
                return NotFound();
            }

            return View(ticketOffer);
        }

        // POST: TicketOffers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketOffer = await _context.TicketOffers.FindAsync(id);
            if (ticketOffer != null)
            {
                _context.TicketOffers.Remove(ticketOffer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketOfferExists(int id)
        {
            return _context.TicketOffers.Any(e => e.Id == id);
        }
    }
}

