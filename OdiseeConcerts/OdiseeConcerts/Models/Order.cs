using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // BELANGRIJK: Nieuwe using directive

namespace OdiseeConcerts.Models
{
    public class Order : BaseEntity
    {
        [Required]
        [Display(Name = "Gebruiker ID")] // Vertaald
        public string UserId { get; set; } = string.Empty; // FK naar AspNetUsers tabel (CustomUser.Id is string)

        // De UserName property is hier VOLLEDIG VERWIJDERD.

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Aantal tickets moet minstens 1 zijn.")]
        [Display(Name = "Aantal Tickets")] // Vertaald
        public int NumTickets { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Gebruik decimal voor valuta
        [Range(0.01, double.MaxValue, ErrorMessage = "Totale prijs moet groter zijn dan 0.")]
        [Display(Name = "Totale Prijs")] // Vertaald
        public decimal TotalPrice { get; set; }

        [Display(Name = "Betaald")] // Vertaald
        public bool Paid { get; set; } = false; // Initiële waarde is false

        [Display(Name = "Korting Toegepast")] // Vertaald
        public bool DiscountApplied { get; set; } = false;

        // Foreign Key
        [Display(Name = "Ticket Aanbieding ID")] // Vertaald
        public int TicketOfferId { get; set; }

        // Navigatie Property
        // BELANGRIJK: [ValidateNever] toegevoegd om validatie van deze navigatieproperty te negeren.
        [ValidateNever] // TOEGEVOEGD: Voorkomt validatie van deze navigatieproperty
        [Display(Name = "Ticket Aanbieding")] // Vertaald
        public TicketOffer TicketOffer { get; set; } = default!; // Geen [Required] meer, want [ValidateNever] negeert validatie.

        // Navigatie property naar de CustomUser die de bestelling heeft geplaatst
        [ForeignKey("UserId")] // Expliciet de foreign key specificeren
        [Display(Name = "Gebruiker")] // Vertaald
        public CustomUser? User { get; set; }
    }
}

