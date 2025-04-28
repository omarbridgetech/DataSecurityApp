using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataSecurityApp.Data;
using DataSecurityApp.Helpers;
using DataSecurityApp.Models;

namespace DataSecurityApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("AccessDenied", "Home");

            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public IActionResult AddUser()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("AccessDenied", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(User user, string password)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("AccessDenied", "Home");

            // Validation for all fields
            bool isValid = true;

            if (!string.IsNullOrEmpty(user.FirstName) && !ValidationHelper.IsValidName(user.FirstName))
            {
                ModelState.AddModelError("FirstName", "First name must contain only letters (no digits or symbols).");
                isValid = false;
            }

            if (!string.IsNullOrEmpty(user.LastName) && !ValidationHelper.IsValidName(user.LastName))
            {
                ModelState.AddModelError("LastName", "Last name must contain only letters (no digits or symbols).");
                isValid = false;
            }

            if (!string.IsNullOrEmpty(user.PersonalId) && !ValidationHelper.IsValidIsraeliId(user.PersonalId))
            {
                ModelState.AddModelError("PersonalId", "Israeli ID must be 9 digits.");
                isValid = false;
            }

            if (!string.IsNullOrEmpty(user.CreditCardNumber) && !ValidationHelper.IsValidCreditCardNumber(user.CreditCardNumber))
            {
                ModelState.AddModelError("CreditCardNumber", "Credit card number must be 16 digits (spaces allowed).");
                isValid = false;
            }

            if (!string.IsNullOrEmpty(user.ValidDate) && !ValidationHelper.IsValidExpirationDate(user.ValidDate))
            {
                ModelState.AddModelError("ValidDate", "Invalid Expiration date.");
                isValid = false;
            }

            if (!string.IsNullOrEmpty(user.CVC) && !ValidationHelper.IsValidCVC(user.CVC))
            {
                ModelState.AddModelError("CVC", "CVC must be 3 or 4 digits.");
                isValid = false;
            }

            if (!isValid)
            {
                return View(user);
            }

            user.PasswordHash = HashHelper.ComputeSha256Hash(password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("AccessDenied", "Home");

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard");
        }
    }
}
