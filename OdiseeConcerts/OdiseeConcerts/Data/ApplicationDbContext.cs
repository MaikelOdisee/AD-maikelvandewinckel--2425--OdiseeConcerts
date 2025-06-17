using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OdiseeConcerts.Models; // <-- Deze using toevoegen!

namespace OdiseeConcerts.Data
{
    public class ApplicationDbContext : IdentityDbContext<CustomUser> // <-- Hier moet <CustomUser> staan!
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
