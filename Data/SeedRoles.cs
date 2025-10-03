using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TradeSphere3.Data
{
    public static class SeedRoles
    {
        public static async Task Initialize(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "User", "Trader" };
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
