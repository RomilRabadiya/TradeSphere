using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using TradeSphere3.Data;
using TradeSphere3.Models;
using TradeSphere3.Repositories;
using TradeSphere3.Mapper; // 👈 Make sure namespace is added where UserTraderMapper lives

namespace TradeSphere3
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // Add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Register DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Add Identity for authentication
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Register Repositories (Scoped is best here ✅)
            services.AddScoped<ITraderRepository, TraderRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IMessageRepositry, MessageRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            // Add MVC
            services.AddControllersWithViews();

            // AutoMapper (registers all Profiles in same assembly as UserTraderMapper ✅)
            services.AddAutoMapper(typeof(UserTraderMapper));
        }

        // Configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Enable Authentication + Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}






//🔎 Why Scoped is best?

//Scoped → A new repository instance per HTTP request.

//✅ Works perfectly with DbContext (which is also scoped by default).

//✅ Prevents memory leaks.

//✅ Ensures all operations in a request share the same DbContext → consistent transactions.

//Alternatives (not recommended here)

//Transient → new instance every time it's requested.

//❌ Can create multiple DbContext instances per request → inconsistent tracking.

//Singleton → single instance for the entire app lifetime.

//❌ Dangerous with DbContext because EF Core DbContext is not thread-safe.