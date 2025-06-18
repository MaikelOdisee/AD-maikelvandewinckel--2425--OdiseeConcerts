using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // BELANGRIJK: Nieuwe using directive

namespace OdiseeConcerts.Models
{
    public class TicketOffer : BaseEntity
    {
        [Required]
        [Display(Name = "Ticket Type")] // Vertaald
        public string TicketType { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Aantal tickets moet een positief getal zijn.")]
        [Display(Name = "Aantal Tickets")] // Vertaald
        public int NumTickets { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Prijs moet groter zijn dan 0.")]
        [Display(Name = "Prijs")] // Vertaald
        public decimal Price { get; set; }

        // Foreign Key
        [Display(Name = "Concert ID")] // Vertaald
        public int ConcertId { get; set; }

        // Navigatie Property
        // BELANGRIJK: [ValidateNever] toegevoegd om validatie te negeren wanneer dit object deel is van een POST.
        [ValidateNever] // TOEGEVOEGD: Voorkomt validatie van deze navigatieproperty
        [Display(Name = "Concert")] // Vertaald
        public Concert? Concert { get; set; } // Is al nullable

        // Navigatie property voor gerelateerde Orders
        public ICollection<Order>? Orders { get; set; }
    }
}