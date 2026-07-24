using Microsoft.AspNetCore.Mvc;
using AurumFinance.Models;
using System.Linq;
using System.Collections.Generic;

namespace AurumFinance.Controllers
{
    public class ChartOfAccountsController : Controller
    {
        // Static list to simulate a database. Replace with your DbContext.
        private static List<ChartOfAccount> _db = new List<ChartOfAccount>
        {
            new ChartOfAccount { Id = 1, ReferenceNumber = 101, AccountName = "Cash on Hand", Type = "Assets", Role = "CashAndEquivalents", Balance = 85000, IsActive = true },
            new ChartOfAccount { Id = 2, ReferenceNumber = 201, AccountName = "Accounts Payable", Type = "Liabilities", Role = "Default", Balance = 15000, IsActive = true },
            new ChartOfAccount { Id = 3, ReferenceNumber = 301, AccountName = "Owner's Capital", Type = "Equity", Role = "Default", Balance = 100000, IsActive = true }
        };

        public IActionResult Index()
        {
            var sortedAccounts = _db.OrderBy(a => a.ReferenceNumber).ToList();
            return View(sortedAccounts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ChartOfAccount model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid data submitted. Please check your inputs.";
                return RedirectToAction(nameof(Index));
            }

            // Validation: Ensure the reference number falls within the correct category range
            bool isNumberValid = ValidateReferenceNumber(model.Type, model.ReferenceNumber);

            if (!isNumberValid)
            {
                TempData["ErrorMessage"] = $"Failed: The reference number {model.ReferenceNumber} is invalid for the '{model.Type}' category.";
                return RedirectToAction(nameof(Index));
            }

            // Validation: Prevent duplicate reference numbers
            if (_db.Any(a => a.ReferenceNumber == model.ReferenceNumber))
            {
                TempData["ErrorMessage"] = $"Failed: Reference number {model.ReferenceNumber} is already in use.";
                return RedirectToAction(nameof(Index));
            }

            model.Id = _db.Any() ? _db.Max(a => a.Id) + 1 : 1;
            model.Balance = 0; 
            _db.Add(model);

            TempData["SuccessMessage"] = $"Account '{model.AccountName}' has been successfully created.";
            return RedirectToAction(nameof(Index));
        }

        private bool ValidateReferenceNumber(string type, int refNumber)
        {
            return type switch
            {
                "Assets" => refNumber >= 100 && refNumber <= 199,
                "Liabilities" => refNumber >= 200 && refNumber <= 299,
                "Equity" => refNumber >= 300 && refNumber <= 399,
                "OperatingIncome" => refNumber >= 400 && refNumber <= 499,
                "OperatingExpenses" => refNumber >= 500 && refNumber <= 599,
                "OtherIncome" => refNumber >= 600 && refNumber <= 799,
                "OtherExpenses" => refNumber >= 800 && refNumber <= 999,
                _ => false
            };
        }
    }
}