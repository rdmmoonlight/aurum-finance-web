using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AuthMvcApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]  // Protected by JWT cookie (you may need custom middleware for cookie JWT validation)
        public IActionResult Welcome()
        {
            ViewData["Message"] = "Selamat datang di aplikasi!";
            return View();
        }
    }
}