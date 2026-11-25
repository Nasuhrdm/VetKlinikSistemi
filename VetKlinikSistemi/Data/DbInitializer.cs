using Microsoft.AspNetCore.Identity;
using VetKlinikSistemi.Models;

namespace VetKlinikSistemi.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "Admin", "Veteriner", "Uye" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "b241200039@sakarya.edu.tr";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Nasuh Erdem (Admin)",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "sau");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            if (!context.Clinics.Any())
            {
                context.Clinics.Add(new Clinic
                {
                    Name = "SAÜ Veteriner Kliniği",
                    Address = "Serdivan, Sakarya",
                    Phone = "0264 295 54 54",
                    OpeningTime = new TimeSpan(9, 0, 0),
                    ClosingTime = new TimeSpan(18, 0, 0)
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
