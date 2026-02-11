using Microsoft.AspNetCore.Identity;

namespace Proj1.Persons;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        string[] roles = { "Admin", "Staff", "Resident" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminEmail = "admin@barangay.gov.ph";
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");
        }
        
        var staffEmail = "staff@barangay.gov.ph";
        var staff = await userManager.FindByEmailAsync(staffEmail);

        if (staff == null)
        {
            staff = new IdentityUser
            {
                UserName = staffEmail,
                Email = staffEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(staff, "Staff@123");
            await userManager.AddToRoleAsync(staff, Roles.Staff);
        }
    }
}