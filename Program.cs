using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TradeSphere3.Data;
using TradeSphere3.Models;

namespace TradeSphere3
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Create a scope to resolve scoped services (DbContext, RoleManager, etc.)
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    logger.LogInformation("Starting TradeSphere3 application initialization...");

                    // Apply pending migrations on startup
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    context.Database.Migrate();

                    // Seed roles (Admin, Trader, etc.)
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    await SeedRoles.Initialize(roleManager);

                    logger.LogInformation("Application initialization completed successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during application initialization.");
                    throw; // Stop application if critical error occurs
                }
            }

            // Run the application
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}