using Microsoft.AspNetCore.Mvc;

namespace AurumFinance.Controllers
{
    public class GeneralJournalController : Controller
    {
        // Route access: /GeneralJournal or /GeneralJournal/Index
        public IActionResult Index()
        {
            ViewData["Title"] = "General Journal";
            return View();
        }
    }
}