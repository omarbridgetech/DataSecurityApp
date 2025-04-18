using Microsoft.AspNetCore.Mvc;

namespace DataSecurityApp.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "User")
                return RedirectToAction("AccessDenied", "Home");

            return View();
        }
    }
}
