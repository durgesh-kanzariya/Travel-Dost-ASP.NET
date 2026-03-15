using Microsoft.AspNetCore.Mvc;
using travel_dost_asp.net.Data;
using travel_dost_asp.net.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace travel_dost_asp.net.Controllers
{
    public class AuthController : Controller
    {
        private readonly TravelDostContext _context;

        public AuthController(TravelDostContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid Credentials";
                return View();
            }

            // Store user info in session
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);
            var role = await (from ur in _context.UserRoles
                             join r in _context.Roles on ur.RoleId equals r.Id
                             where ur.UserId == user.Id
                             select r.RoleName).FirstOrDefaultAsync();

            var userData = new {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = role?.ToLower() ?? "user",
                nativeLanguage = profile?.NativeLanguage ?? "English"
            };

            HttpContext.Session.SetString("user", JsonSerializer.Serialize(userData));
            
            if (userData.role == "admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(string firstName, string lastName, string email, string password, string nativeLanguage)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "User already exists";
                return View();
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var profile = new UserProfile
            {
                UserId = user.Id,
                NativeLanguage = nativeLanguage,
                DefaultCurrency = "USD"
            };
            _context.UserProfiles.Add(profile);

            // Assign default 'User' role
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            if (userRole != null)
            {
                _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = userRole.Id });
            }

            await _context.SaveChangesAsync();

            // Set session and redirect
            var userData = new {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = "user",
                nativeLanguage = nativeLanguage
            };
            HttpContext.Session.SetString("user", JsonSerializer.Serialize(userData));

            return RedirectToAction("Dashboard", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
