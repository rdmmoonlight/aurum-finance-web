using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AurumFinance.Models;

namespace AurumFinance.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterModel();
            return View(model);
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email address is already registered.");
                return View(model);
            }

            var newUser = new User
            {
                Email = model.Email,
                FullName = model.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful! Please sign in.";
            return RedirectToAction("Index", "Home");
        }
    }
}