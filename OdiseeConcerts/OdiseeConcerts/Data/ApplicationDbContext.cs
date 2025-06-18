using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OdiseeConcerts.Models; // Zorg dat deze using aanwezig is

namespace OdiseeConcerts.Data
{
    public class ApplicationDbContext : IdentityDbContext<CustomUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            // Optioneel: Verhoog de time-out voor database commando's (voor debugging van hangende operaties)
            // Standaard is dit 30 seconden. Verhoog dit naar bijvoorbeeld 120 seconden om te zien of het een time-out is.
            Database.SetCommandTimeout(120); // ZET DEZE REGEL HIER OM DE TIME-OUT IN TE STELLEN
        }

        // ===============================================
        // NIEUWE DbSet PROPERTIES TOEVOEGEN
        // ===============================================
        public DbSet<Concert> Concerts { get; set; } = default!;
        public DbSet<TicketOffer> TicketOffers { get; set; } = default!;
        public DbSet<Order> Orders { get; set; } = default!;
        // ===============================================

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Dit is cruciaal! Roep eerst de base-implementatie aan om Identity-tabellen te configureren.
            base.OnModelCreating(builder);

            // Configureer de relatie tussen Order en CustomUser
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany() // Eén gebruiker kan meerdere orders plaatsen
                .HasForeignKey(o => o.UserId)
                .IsRequired(); // AANGEPAST: UserId is 'required' volgens Order.cs model [Required] attribuut

            // Configureer relaties tussen Concert en TicketOffer
            builder.Entity<Concert>()
                .HasMany(c => c.TicketOffers)
                .WithOne(to => to.Concert)
                .HasForeignKey(to => to.ConcertId)
                .OnDelete(DeleteBehavior.Cascade); // Optioneel: Als een concert wordt verwijderd, verwijder dan ook de TicketOffers

            // Configureer relaties tussen TicketOffer en Order
            builder.Entity<TicketOffer>()
                .HasMany(to => to.Orders)
                .WithOne(o => o.TicketOffer)
                .HasForeignKey(o => o.TicketOfferId)
                .OnDelete(DeleteBehavior.Restrict); // Optioneel: Voorkom verwijdering TicketOffer als er Orders zijn

            // Optioneel: Forceer Dateonly voor Date field in Concert voor alleen datum (zonder tijd)
            // Dit vereist Microsoft.EntityFrameworkCore.SqlServer en kan afhangen van je precieze databasebehoeften.
            // builder.Entity<Concert>()
            //      .Property(c => c.Date)
            //      .HasColumnType("date");

            // ===============================================
            // START SEEDING DATA VOOR Concerts en TicketOffers
            // ===============================================

            builder.Entity<Concert>().HasData(
                new Concert { Id = 1, Artist = "Taylor Swift", Location = "Koning Boudewijn Stadion, Brussel", Date = new DateTime(2025, 03, 15) },
                new Concert { Id = 2, Artist = "Taylor Swift", Location = "Koning Boudewijn Stadion, Brussel", Date = new DateTime(2025, 03, 16) },
                new Concert { Id = 3, Artist = "Charli XCX", Location = "Vorst Nationaal, Brussel", Date = new DateTime(2025, 04, 16) },
                new Concert { Id = 4, Artist = "Compact Disk Dummies", Location = "Ancienne Belgique, Brussel", Date = new DateTime(2025, 04, 26) },
                new Concert { Id = 5, Artist = "Compact Disk Dummies", Location = "Ancienne Belgique, Brussel", Date = new DateTime(2025, 04, 27) },
                new Concert { Id = 6, Artist = "Coldplay", Location = "Sportpaleis, Antwerpen", Date = new DateTime(2025, 05, 07) },
                new Concert { Id = 7, Artist = "Dua Lipa", Location = "Werchter", Date = new DateTime(2025, 06, 18) },
                new Concert { Id = 8, Artist = "Dua Lipa", Location = "Werchter", Date = new DateTime(2025, 06, 19) } // Aangepast naar 19/06/2025 voor uniciteit t.o.v. Concert 7
            );

            builder.Entity<TicketOffer>().HasData(
                // Concert 1 (Taylor Swift - 15/03/2025)
                new TicketOffer { Id = 1, TicketType = "Golden Circle", NumTickets = 10, Price = 200m, ConcertId = 1 },
                new TicketOffer { Id = 2, TicketType = "Standing", NumTickets = 50, Price = 50m, ConcertId = 1 },
                new TicketOffer { Id = 3, TicketType = "Seated", NumTickets = 60, Price = 60m, ConcertId = 1 },

                // Concert 2 (Taylor Swift - 16/03/2025)
                new TicketOffer { Id = 4, TicketType = "Golden Circle", NumTickets = 1000, Price = 200m, ConcertId = 2 },
                new TicketOffer { Id = 5, TicketType = "Standing", NumTickets = 19000, Price = 50m, ConcertId = 2 },
                new TicketOffer { Id = 6, TicketType = "Seated", NumTickets = 20000, Price = 60m, ConcertId = 2 },

                // Concert 3 (Charli XCX - 16/04/2025)
                new TicketOffer { Id = 7, TicketType = "Golden Circle", NumTickets = 0, Price = 100m, ConcertId = 3 },
                new TicketOffer { Id = 8, TicketType = "Standing", NumTickets = 0, Price = 28m, ConcertId = 3 },
                new TicketOffer { Id = 9, TicketType = "Seated", NumTickets = 0, Price = 32m, ConcertId = 3 },

                // Concert 4 (Compact Disk Dummies - 26/04/2025)
                new TicketOffer { Id = 10, TicketType = "Standing", NumTickets = 2000, Price = 28m, ConcertId = 4 },
                new TicketOffer { Id = 11, TicketType = "Seated", NumTickets = 1800, Price = 32m, ConcertId = 4 },

                // Concert 5 (Compact Disk Dummies - 27/04/2025)
                new TicketOffer { Id = 12, TicketType = "Standing", NumTickets = 2000, Price = 28m, ConcertId = 5 },
                new TicketOffer { Id = 13, TicketType = "Seated", NumTickets = 7800, Price = 32m, ConcertId = 5 },

                // Concert 6 (Coldplay - 07/05/2025)
                new TicketOffer { Id = 14, TicketType = "Golden Circle", NumTickets = 400, Price = 150m, ConcertId = 6 },
                new TicketOffer { Id = 15, TicketType = "Standing", NumTickets = 4000, Price = 65m, ConcertId = 6 },
                new TicketOffer { Id = 16, TicketType = "Seated", NumTickets = 4000, Price = 55m, ConcertId = 6 },

                // Concert 7 (Dua Lipa - 18/06/2025)
                new TicketOffer { Id = 17, TicketType = "Golden Circle", NumTickets = 1000, Price = 124m, ConcertId = 7 },
                new TicketOffer { Id = 18, TicketType = "Standing", NumTickets = 20000, Price = 67m, ConcertId = 7 },

                // Concert 8 (Dua Lipa - 19/06/2025)
                new TicketOffer { Id = 19, TicketType = "Standing", NumTickets = 2000, Price = 36m, ConcertId = 8 },
                new TicketOffer { Id = 20, TicketType = "Seated", NumTickets = 7800, Price = 40m, ConcertId = 8 }
            );

            // ===============================================
            // EINDE SEEDING DATA
            // ===============================================
        }
    }
}

