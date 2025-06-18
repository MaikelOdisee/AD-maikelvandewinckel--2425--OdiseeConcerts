using Microsoft.AspNetCore.Identity;
using OdiseeConcerts.Interfaces;
using OdiseeConcerts.Models;
using OdiseeConcerts.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Toegevoegd voor DbUpdateConcurrencyException

namespace OdiseeConcerts.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITicketOfferRepository _ticketOfferRepository;
        private readonly UserManager<CustomUser> _userManager;
        private const decimal MemberCardDiscountPercentage = 0.10m; // 10% korting voor leden

        public OrderService(IOrderRepository orderRepository, ITicketOfferRepository ticketOfferRepository, UserManager<CustomUser> userManager)
        {
            _orderRepository = orderRepository;
            _ticketOfferRepository = ticketOfferRepository;
            _userManager = userManager;
        }

        public async Task<int> CreateOrder(OrderFormViewModel model)
        {
            Console.WriteLine($"[OrderService] Attempting to create order for TicketOfferId: {model.TicketOfferId}, NumberOfTickets: {model.NumberOfTickets}, UserId: {model.UserId}");

            var ticketOffer = _ticketOfferRepository.GetTicketOfferWithConcertById(model.TicketOfferId);

            if (ticketOffer == null)
            {
                Console.WriteLine($"[OrderService] Error: TicketOffer with ID {model.TicketOfferId} not found.");
                return 0; // TicketOffer niet gevonden
            }

            if (ticketOffer.NumTickets < model.NumberOfTickets)
            {
                Console.WriteLine($"[OrderService] Error: Not enough tickets available. Requested: {model.NumberOfTickets}, Available: {ticketOffer.NumTickets}");
                return 0; // Niet genoeg tickets beschikbaar
            }

            decimal finalPricePerTicket = ticketOffer.Price;
            bool discountApplied = false;

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                Console.WriteLine($"[OrderService] Error: User with ID {model.UserId} not found.");
                return 0; // Gebruiker niet gevonden, dit mag normaal niet gebeuren als ingelogd
            }

            if (user.HasMemberCard)
            {
                finalPricePerTicket -= (ticketOffer.Price * MemberCardDiscountPercentage);
                discountApplied = true;
                Console.WriteLine($"[OrderService] Member card applied. New finalPricePerTicket: {finalPricePerTicket:F2}");
            }

            decimal finalTotalPrice = finalPricePerTicket * model.NumberOfTickets;

            var order = new Order
            {
                UserId = model.UserId,
                TicketOfferId = model.TicketOfferId,
                NumTickets = model.NumberOfTickets,
                TotalPrice = finalTotalPrice, // Gebruik de server-berekende prijs
                Paid = false,
                DiscountApplied = discountApplied,
                OrderDate = DateTime.UtcNow
            };

            try
            {
                Console.WriteLine($"[OrderService] Adding order to repository for TicketOfferId: {order.TicketOfferId}, NumTickets: {order.NumTickets}, TotalPrice: {order.TotalPrice:F2}");
                await _orderRepository.AddOrder(order); // Bestelling toevoegen
                Console.WriteLine($"[OrderService] Order added successfully with ID: {order.Id}");

                // Update het aantal beschikbare tickets
                ticketOffer.NumTickets -= model.NumberOfTickets;
                Console.WriteLine($"[OrderService] Updating TicketOffer {ticketOffer.Id} (Concert: {ticketOffer.Concert?.Artist ?? "N/A"}) new NumTickets: {ticketOffer.NumTickets}");
                await _ticketOfferRepository.UpdateTicketOffer(ticketOffer); // Ticketaanbod bijwerken
                Console.WriteLine($"[OrderService] TicketOffer updated successfully.");

                return order.Id; // Bestelling succesvol, retourneer het ID
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] FATAL ERROR during order creation or ticket update: {ex.Message}");
                Console.WriteLine($"[OrderService] Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[OrderService] Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"[OrderService] Inner Exception Stack Trace: {ex.InnerException.StackTrace}");
                }
                return 0; // Er is een fout opgetreden
            }
        }

        public OrderViewModel? GetOrderById(int orderId)
        {
            var order = _orderRepository.GetOrderWithDetailsById(orderId);

            if (order == null)
            {
                return null;
            }

            return new OrderViewModel
            {
                Id = order.Id,
                NumTickets = order.NumTickets,
                TotalPrice = order.TotalPrice,
                Paid = order.Paid,
                DiscountApplied = order.DiscountApplied,
                TicketType = order.TicketOffer?.TicketType ?? "Onbekend",
                PricePerTicket = order.TicketOffer?.Price ?? 0,
                ConcertArtist = order.TicketOffer?.Concert?.Artist ?? "Onbekend",
                ConcertLocation = order.TicketOffer?.Concert?.Location ?? "Onbekend",
                ConcertDate = order.TicketOffer?.Concert?.Date ?? DateTime.MinValue,
                UserEmail = order.User?.Email ?? "Onbekend",
                UserFullName = $"{order.User?.FirstName} {order.User?.LastName}".Trim()
            };
        }

        /// <summary>
        /// Werkt de betaalstatus van een bestelling bij.
        /// </summary>
        /// <param name="orderId">Het ID van de bestelling.</param>
        /// <param name="paid">De nieuwe betaalstatus (true voor betaald, false voor onbetaald).</param>
        /// <returns>True als de update succesvol was, anders False.</returns>
        public async Task<bool> UpdatePaid(int orderId, bool paid)
        {
            var order = _orderRepository.GetOrderById(orderId);
            if (order == null)
            {
                Console.WriteLine($"[OrderService] Warning: Order with ID {orderId} not found for UpdatePaid.");
                return false;
            }

            // Alleen updaten als de status wijzigt
            if (order.Paid == paid)
            {
                Console.WriteLine($"[OrderService] Order {orderId} already has paid status {paid}. No update needed.");
                return true; // Al de gewenste status, beschouw als succes
            }

            order.Paid = paid;
            try
            {
                await _orderRepository.UpdateOrder(order);
                Console.WriteLine($"[OrderService] Order {orderId} paid status updated to {paid}.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] ERROR: Failed to update paid status for order {orderId}. Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Bevestigt de betaling voor een specifieke bestelling.
        /// </summary>
        /// <param name="orderId">Het ID van de bestelling waarvan de betaling moet worden bevestigd.</param>
        /// <returns>True als de betaling succesvol is bevestigd, anders False.</returns>
        public async Task<bool> ConfirmOrderPaymentAsync(int orderId)
        {
            // Direct gebruik maken van de bestaande en nu robuustere UpdatePaid methode
            return await UpdatePaid(orderId, true);
        }
    }
}