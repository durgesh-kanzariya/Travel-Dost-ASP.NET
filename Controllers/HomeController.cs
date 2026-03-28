using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using travel_dost_asp.net.Models;
using travel_dost_asp.net.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace travel_dost_asp.net.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TravelDostContext _context;

        public HomeController(ILogger<HomeController> _logger, TravelDostContext context)
        {
            this._logger = _logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var userJson = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userJson))
            {
                return RedirectToAction("Login", "Auth");
            }

            var userData = JsonSerializer.Deserialize<JsonElement>(userJson);
            ViewBag.User = userData;

            int userId = userData.GetProperty("id").GetInt32();
            var recentTrips = await _context.Trips
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.StartDate)
                .Take(2)
                .ToListAsync();

            return View(recentTrips);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
