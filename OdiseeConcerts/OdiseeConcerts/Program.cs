using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OdiseeConcerts.Data;
using OdiseeConcerts.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization; // Toegevoegd voor AddAuthorization
using OdiseeConcerts.Interfaces; // TOEGEVOEGD: Nodig voor je interfaces
using OdiseeConcerts.Repositories; // TOEGEVOEGD: Nodig voor je repositories
using OdiseeConcerts.Services; // TOEGEVOEGD: Nodig voor je services

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<CustomUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ===============================================
// START NIEUWE CODE: IsAdmin Policy Configuratie
// ===============================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsAdmin", policy =>
        policy.RequireClaim("IsAdmin", "true"));
});
// ===============================================
// EINDE NIEUWE CODE
// ===============================================

// ===============================================
// TOEGEVOEGD: Dependency Injection voor Repositories en Services
// Registreer je interfaces en hun implementaties hier
// ===============================================
builder.Services.AddScoped<IConcertRepository, ConcertRepository>();
builder.Services.AddScoped<IConcertService, ConcertService>();
builder.Services.AddScoped<ITicketOfferRepository, TicketOfferRepository>(); // TOEGEVOEGD
builder.Services.AddScoped<ITicketOfferService, TicketOfferService>();     // TOEGEVOEGD
builder.Services.AddScoped<IOrderRepository, OrderRepository>();           // TOEGEVOEGD
builder.Services.AddScoped<IOrderService, OrderService>();                 // TOEGEVOEGD
// ===============================================

builder.Services.AddControllersWithViews(); // Deze stond er al, zorg dat AddAuthorization erboven staat

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// ===============================================
// Start Admin Seeding Code (bestaande code, onaangeroerd)
// ===============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<CustomUser>>();
        // Indien je later met Identity Roles wilt werken, voeg je RoleManager toe en uncomment de regel hieronder:
        // var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        string adminEmail = "admin@odiseeconcerts.be"; // Kies een uniek admin e-mailadres
        string adminPassword = "AdminPassword123!"; // **LET OP: Wijzig dit wachtwoord direct en gebruik een sterk wachtwoord!**

        // Controleer of de admin gebruiker al bestaat
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            // Gebruiker bestaat nog niet, maak deze aan
            adminUser = new CustomUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "Gebruiker",
                MemberCardNumber = "ODI1234567890" // Optioneel
            };

            var createPowerUser = await userManager.CreateAsync(adminUser, adminPassword);
            if (createPowerUser.Succeeded)
            {
                // Indien je rollen gebruikt, uncomment dan deze lijn:
                // await userManager.AddToRoleAsync(adminUser, "Admin"); // Voeg toe aan Admin rol
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Admin gebruiker succesvol aangemaakt.");

                // Voeg de IsAdmin claim toe
                await userManager.AddClaimAsync(adminUser, new Claim("IsAdmin", "true"));
                logger.LogInformation("Claim 'IsAdmin=true' toegevoegd aan Admin gebruiker.");
            }
            else
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                var errors = string.Join(", ", createPowerUser.Errors.Select(e => e.Description));
                logger.LogError($"Fout bij het aanmaken van Admin gebruiker: {errors}");
            }
        }
        else
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Admin gebruiker bestaat al.");

            // Controleer of de IsAdmin claim al bestaat, zo niet, voeg toe
            var existingClaims = await userManager.GetClaimsAsync(adminUser);
            if (!existingClaims.Any(c => c.Type == "IsAdmin" && c.Value == "true"))
            {
                await userManager.AddClaimAsync(adminUser, new Claim("IsAdmin", "true"));
                logger.LogInformation("Claim 'IsAdmin=true' toegevoegd aan bestaande Admin gebruiker.");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Er is een fout opgetreden bij het seeden van de admin gebruiker.");
    }
}
// ===============================================
// Einde Admin Seeding Code
// ===============================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Concerts}/{action=Index}/{id?}"); // AANGEPAST: Standaard controller is nu Concerts
app.MapRazorPages();

app.Run();