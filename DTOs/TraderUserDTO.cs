using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;


namespace TradeSphere3.DTOs
{
    public class TraderDto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(200)]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Remote(action: "IsEmailAvailable", controller: "Trader", ErrorMessage = "This email is already registered as a Trader.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Trade role is required")]
        [StringLength(20)]
        public string TradeRole { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "CIN is required")]
        public string CIN { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "GSTNo is required")]
        public string GSTNo { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "ISO is required")]
        public string ISO { get; set; }
        
        [Required(ErrorMessage = "Country is required")]
        [StringLength(100)]
        public string Country { get; set; }
        
        [StringLength(100)]
        public string State { get; set; }

        [StringLength(100)]
        public string City { get; set; }
        
        [StringLength(300)]
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Required(ErrorMessage = "TurnOver is required")]
        public decimal Turnover { get; set; }

        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; }

        public int TrustScore { get; set; }

    }

    public class UserWithTraderDto
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public TraderDto Trader { get; set; }
    }
}
























//Great question 👌 let’s break this down step by step:

//---

//## 🔹 Option 1: **Directly use Entity (`Trader`) in Controller/View**

//*You fetch a `Trader` from DB (`context.Traders.Include(x => x.User)...`)
//* You pass that **entity object** to the View.
//* Example:

//  ```csharp
//  public IActionResult Profile()
//{
//    var userId = _userManager.GetUserId(User);
//    var trader = _context.Traders
//                        .Include(t => t.User)
//                        .FirstOrDefault(t => t.UserId == userId);
//    return View(trader);
//}
//  ```

//✅ **Pros * *

//*Very simple, fewer classes.
//* Faster to implement (less code).

//❌ **Cons**

//* **Security risk** → entity has properties like `UserId`, `Status`, maybe even navigation objects you don’t want exposed in the View.
//* **Tight coupling** → any DB change (renaming column, adding field) will break UI.
//* **Validation mess** → you mix DB rules and UI rules (hard to manage).

//---

//## 🔹 Option 2: **Use DTOs + AutoMapper**

//* Controller fetches `Trader` entity.
//* AutoMapper maps it to `TraderDto`.
//* You pass **only safe properties** to the View.
//* Example:

//  ```csharp
//  public IActionResult Profile()
//{
//    var userId = _userManager.GetUserId(User);
//    var trader = _context.Traders
//                        .Include(t => t.User)
//                        .FirstOrDefault(t => t.UserId == userId);

//    var dto = _mapper.Map<TraderDto>(trader);
//    return View(dto);
//}
//  ```

//✅ **Pros * *

//***Security - safe * * → you only expose what you want.
//* **Separation of concerns** → DB layer & UI layer independent.
//* **Scalability** → if tomorrow you build an **API** or add a **mobile app**, the DTOs will already be there.
//* **Cleaner validations** → keep DB annotations separate from UI validations.

//❌ **Cons**

//* More classes (extra DTOs).
//* Need AutoMapper setup.

//---

//## 🔹 Which is better for our project (Trader + User role system)?

//👉 **DTO approach is better** for **medium-to-large projects** like yours because:

//*You already have multiple roles (`User`, `Trader`).
//* You are building dashboards & role-based UIs (could expand).
//* It makes sense to protect DB entities and give controlled data to the front-end.

//👉 **Entity approach is OK** only if:

//*Project is **small & internal** (like a quick admin panel).
//* You don’t care much about separating DB and UI.

//---

//⚖️ **Final Recommendation for your project:**
//✅ Go with **DTO + AutoMapper** → cleaner, safer, future-proof.
//❌ Avoid passing `Trader` entity directly to Views, it will cause tight coupling & security leaks down the line.

//---

//Do you want me to now **redesign your Trader Apply + Dashboard flow using DTOs**(with AutoMapper mappings), so you see how the implementation changes compared to your current entity-based approach?
