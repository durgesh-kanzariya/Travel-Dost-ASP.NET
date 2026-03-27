using Microsoft.AspNetCore.Mvc;
using travel_dost_asp.net.Data;
using travel_dost_asp.net.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace travel_dost_asp.net.Controllers
{
    public class AdminController : Controller
    {
        private readonly TravelDostContext _context;

        public AdminController(TravelDostContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            var userJson = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userJson)) return false;
            var userData = JsonSerializer.Deserialize<JsonElement>(userJson);
            return userData.TryGetProperty("role", out var role) && role.GetString() == "admin";
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Dashboard", "Home");

            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalGuides = await _context.CountryGuides.CountAsync();

            return View();
        }

        public async Task<IActionResult> Users()
        {
            if (!IsAdmin()) return RedirectToAction("Dashboard", "Home");

            // Fetch users with roles and profiles using LINQ Select so that [NotMapped] properties are projected
            var users = await _context.Users
                .Select(u => new User {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PasswordHash = u.PasswordHash,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Role = (from ur in _context.UserRoles
                            join r in _context.Roles on ur.RoleId equals r.Id
                            where ur.UserId == u.Id
                            select r.RoleName).FirstOrDefault() == null ? "user" : (from ur in _context.UserRoles
                                                                                    join r in _context.Roles on ur.RoleId equals r.Id
                                                                                    where ur.UserId == u.Id
                                                                                    select r.RoleName).FirstOrDefault().ToLower(),
                    NativeLanguage = _context.UserProfiles.Where(up => up.UserId == u.Id).Select(up => up.NativeLanguage).FirstOrDefault(),
                    DefaultCurrency = _context.UserProfiles.Where(up => up.UserId == u.Id).Select(up => up.DefaultCurrency).FirstOrDefault()
                })
                .AsNoTracking()
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Guides()
        {
            if (!IsAdmin()) return RedirectToAction("Dashboard", "Home");

            var guides = await _context.CountryGuides.OrderBy(g => g.CountryName).ToListAsync();
            return View(guides);
        }

        [HttpPost]
        public async Task<IActionResult> AddGuide(string countryName, string policeNumber, string ambulanceNumber, string fireNumber, string embassyNumber, string localRules)
        {
            if (!IsAdmin()) return Unauthorized();

            var guide = new CountryGuide
            {
                CountryName = countryName,
                PoliceNumber = policeNumber,
                AmbulanceNumber = ambulanceNumber,
                FireNumber = fireNumber,
                EmbassyNumber = embassyNumber,
                LocalRules = localRules?.Split('\n').Select(r => r.Trim()).ToList() ?? new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CountryGuides.Add(guide);
            await _context.SaveChangesAsync();

            return RedirectToAction("Guides");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGuide(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var guide = await _context.CountryGuides.FindAsync(id);
            if (guide == null) return NotFound();

            _context.CountryGuides.Remove(guide);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Check if deleting self
            var currentUserJson = HttpContext.Session.GetString("user");
            var currentUserId = JsonSerializer.Deserialize<JsonElement>(currentUserJson).GetProperty("id").GetInt32();
            if (id == currentUserId) return BadRequest("Cannot delete yourself");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
