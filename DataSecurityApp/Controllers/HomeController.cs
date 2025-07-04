using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataSecurityApp.Data;
using DataSecurityApp.Helpers;
using DataSecurityApp.Models;
using System.Net.Mail;
using System.Net;

namespace DataSecurityApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context) => _context = context;

        public IActionResult Index() => View();


        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            User user = null;

            // First try normal login (secure EF Core approach)
            var hash = HashHelper.ComputeSha256Hash(password);
            user = _context.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == hash);

            // If normal login fails, still use a SECURE approach for the direct SQL
            if (user == null)
            {
                try
                {
                    using (var connection = new Microsoft.Data.SqlClient.SqlConnection(_context.Database.GetConnectionString()))
                    {
                        connection.Open();

                        // SECURE APPROACH: Use parameterized query
                        string query = "SELECT * FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";

                        using (var command = new Microsoft.Data.SqlClient.SqlCommand(query, connection))
                        {
                            // Add parameters instead of concatenating strings
                            command.Parameters.AddWithValue("@Username", username);
                            command.Parameters.AddWithValue("@PasswordHash", password); // Or hash if needed

                            try
                            {
                                using (var reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        user = new User
                                        {
                                            Id = (int)reader["Id"],
                                            Username = reader["Username"].ToString(),
                                            Email = reader["Email"].ToString(),
                                            Role = reader["Role"].ToString()
                                        };
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"SQL Error: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "An error occurred during login.";
                    return View("Index");
                }
            }

            // Rest of your login logic remains the same
            if (user != null)
            {
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("Username", user.Username);

                return RedirectToAction("Dashboard", user.Role == "Admin" ? "Admin" : "User");
            }

            ViewBag.Error = "Invalid login";
            return View("Index");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "Email not found";
                return View();
            }

            string token = Guid.NewGuid().ToString();
            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                Email = email,
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1)
            });
            await _context.SaveChangesAsync();

            var resetLink = Url.Action("ResetPassword", "Home", new { token = token }, Request.Scheme);

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("omar99awaisha@gmail.com", "dypr vazs dxnp oskg"),
                EnableSsl = true,
            };

            smtpClient.Send("omar99awaisha@gmail.com", email, "Password Reset", $"Click here to reset your password: {resetLink}");

            ViewBag.Message = "Reset link sent to your email.";
            return View();
        }

        public IActionResult ResetPassword(string token) => View(model: token);

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {
            var resetToken = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == token && t.Expiration > DateTime.UtcNow);
            if (resetToken == null) return Content("Invalid or expired token");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetToken.Email);
            if (user == null) return Content("User not found");

            user.PasswordHash = HashHelper.ComputeSha256Hash(newPassword);
            _context.PasswordResetTokens.Remove(resetToken);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult AccessDenied() => View();
    }
}
