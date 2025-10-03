using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TradeSphere3.DTOs;
using TradeSphere3.Models;
using TradeSphere3.Repositories;
using TradeSphere3.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeSphere3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace TradeSphere3.Controllers
{
    public class TraderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly RoleManager<IdentityRole> _roleManager;

    public TraderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper, 
        IProductRepository productRepository, IOrderRepository orderRepository)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Apply()
    {
        return View(new TraderDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    //[Authorize(Roles = "Trader")]
    public async Task<IActionResult> Apply(TraderDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        //var user = await _userManager.GetUserAsync(User);

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            // Ensure Trader role exists
            if (!await _roleManager.RoleExistsAsync("Trader"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Trader"));
            }

            // Remove old role (User)
            if (await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
            }

            // Add new role (Trader)
            if (!await _userManager.IsInRoleAsync(user, "Trader"))
            {
                await _userManager.AddToRoleAsync(user, "Trader");
            }
        }

        var trader = _mapper.Map<Trader>(dto);
        trader.UserId = user.Id;
        trader.RegistrationDate = DateTime.Now;

        _context.Traders.Add(trader);
        await _context.SaveChangesAsync();

        // update user to mark as trader
        user.Trader = trader;
        await _userManager.UpdateAsync(user);

        TempData["Message"] = "Trader application submitted successfully!";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize(Roles = "Trader")]
    public async Task<IActionResult> MyProfile()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        // Load Trader profile with user info
        var userWithTrader = await _context.Users
            .Include(u => u.Trader)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (userWithTrader?.Trader == null)
        {
            TempData["Error"] = "You have not registered as a trader yet.";
            return RedirectToAction("Apply");
        }

        // Map to DTO (if you use DTOs)
        var dto = _mapper.Map<UserWithTraderDto>(userWithTrader);

        return View(dto);
    }



    [HttpGet]
    [Authorize(Roles = "Trader")]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null || user.Trader == null)
            return RedirectToAction("Apply"); // If not a trader yet, redirect to apply

        var dto = _mapper.Map<TraderDto>(user.Trader);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Trader")]
    public async Task<IActionResult> Edit(TraderDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var user = await _userManager.GetUserAsync(User);

        if (user == null || user.Trader == null)
            return RedirectToAction("Apply");

        // Update trader profile
        _mapper.Map(dto, user.Trader);

        await _context.SaveChangesAsync();

        TempData["Message"] = "Trader profile updated successfully!";
        return RedirectToAction("MyProfile");

    }











        // Products Management
        [Authorize(Roles = "Trader")]
        public IActionResult Products()
        {
            try
            {
                // For now, get all products - later we'll filter by trader
                var products = _productRepository.GetAll();
                return View(products);
            }
            catch (Exception)
            {
                // Return empty list if database error
                return View(new List<Product>());
            }
        }

        [Authorize(Roles = "Trader")]
        public IActionResult CreateProduct()
        {
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "Trader")]
        public IActionResult CreateProduct(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Category = model.Category,
                    Price = model.Price,
                    Quantity = model.Quantity,
                    Unit = model.Unit,
                    Status = model.Status,
                    TraderId = 1 // For now hardcode - later get from current trader
                };

                _productRepository.Add(product);
                return RedirectToAction("Products");
            }
            return View(model);
        }

        // Orders Management
        [Authorize(Roles = "Trader")]
        public IActionResult Orders()
        {
            try
            {
                var orders = _orderRepository.GetAll();
                return View(orders);
            }
            catch (Exception)
            {
                // Return empty list if database error
                return View(new List<Order>());
            }
        }



        
    }
}
