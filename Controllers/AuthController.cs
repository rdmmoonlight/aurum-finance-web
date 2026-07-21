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

            // 1. Cek apakah email sudah terdaftar di database
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email address is already registered.");
                return View(model);
            }

            // 2. Buat entity User baru
            var newUser = new User
            {
                Email = model.Email,
                FullName = model.FullName,
                // Sesuai best practice, lakukan hashing password di sini sebelum disimpan
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password) 
            };

            // 3. Simpan ke database via EF Core
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful! Please sign in.";
            return RedirectToAction("Index", "Home");
        }
    }
}