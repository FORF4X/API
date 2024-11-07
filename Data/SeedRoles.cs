using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace API.Data
{
    public static class SeedRoles
    {
        public static async Task Seed(RoleManager<IdentityRole> roleManager)
        {
            // Check if the User role exists and create it if not
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Check if the Doctor role exists and create it if not
            if (!await roleManager.RoleExistsAsync("Doctor"))
            {
                await roleManager.CreateAsync(new IdentityRole("Doctor"));
            }
        }
    }
}