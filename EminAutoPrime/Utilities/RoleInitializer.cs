using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace EminAutoPrime.Utilities
{
    public class RoleInitializer
    {
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Müşteri", "ServisCalisani" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
