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

            user.PasswordHash = HashHelper.ComputeSha256Hash(password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
    }
}
