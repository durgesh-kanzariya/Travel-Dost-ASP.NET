using Microsoft.AspNetCore.Mvc;
using travel_dost_asp.net.Data;
using travel_dost_asp.net.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace travel_dost_asp.net.Controllers
{
    public class SettingsController : Controller
    {
        private readonly TravelDostContext _context;

        public SettingsController(TravelDostContext context)
        {
            _context = context;
        }

        private int? GetUserId()
        {
            var userJson = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userJson)) return null;
            var userData = JsonSerializer.Deserialize<JsonElement>(userJson);
            return userData.GetProperty("id").GetInt32();
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(userId);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string nativeLanguage, string defaultCurrency)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FirstName = firstName;
            user.LastName = lastName;
            user.NativeLanguage = nativeLanguage;
            user.DefaultCurrency = defaultCurrency;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update session
            var userJson = JsonSerializer.Serialize(user);
            HttpContext.Session.SetString("user", userJson);

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("Index");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Password changed successfully!";
            return RedirectToAction("Index");
        }
    }
}
