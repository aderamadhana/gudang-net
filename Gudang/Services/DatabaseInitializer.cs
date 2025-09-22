using Gudang.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace Gudang.Services
{
    public static class DatabaseInitializer
    {
        public static async Task SendData(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            if (userManager == null || roleManager == null)
            {
                Console.WriteLine("UserManager or RoleManager is null => exit");
                return;
            }

            // 1) Ensure role exists
            if (!await roleManager.RoleExistsAsync("admin"))
            {
                Console.WriteLine("Creating 'admin' role...");
                var roleResult = await roleManager.CreateAsync(new IdentityRole("admin"));
                if (!roleResult.Succeeded)
                {
                    Console.WriteLine($"Failed to create role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    return;
                }
            }

            // 2) Ensure at least one admin user
            var adminUsers = await userManager.GetUsersInRoleAsync("admin");
            if (adminUsers.Any())
            {
                Console.WriteLine("An admin user already exists. Skipping user creation.");
                return;
            }

            Console.WriteLine("No admin user found. Creating default admin user...");

            var adminEmail = "admin@yourapp.local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, "ChangeThis!123"); // TODO: move to secrets
                if (!createResult.Succeeded)
                {
                    Console.WriteLine($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    return;
                }
            }

            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "admin");
            if (!addToRoleResult.Succeeded)
            {
                Console.WriteLine($"Failed to add admin role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                return;
            }

            Console.WriteLine("Default admin user ensured.");
        }
    }

}
