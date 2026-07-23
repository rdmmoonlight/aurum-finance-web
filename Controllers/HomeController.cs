using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AurumFinance.Controllers
{
    public class HomeController : Controller
    {
[AllowAnonymous] // Biar bisa ditembak tamu
    public IActionResult Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Auth");
        }
        return View();
    }
    }
}