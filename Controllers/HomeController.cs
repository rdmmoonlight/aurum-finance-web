using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AurumFinance.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]  // Requires a signed-in cookie session — see Controllers/AuthController.cs and Program.cs.
        public IActionResult Welcome()
        {
            ViewData["Message"] = "Selamat datang di aplikasi!";
            return View();
        }
    }
}