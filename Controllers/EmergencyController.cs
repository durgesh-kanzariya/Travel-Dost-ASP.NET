using Microsoft.AspNetCore.Mvc;
using travel_dost_asp.net.Data;
using Microsoft.EntityFrameworkCore;

namespace travel_dost_asp.net.Controllers
{
    public class EmergencyController : Controller
    {
        private readonly TravelDostContext _context;

        public EmergencyController(TravelDostContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("user")))
            {
                return RedirectToAction("Login", "Auth");
            }

            var countries = await _context.CountryGuides
                .Select(g => g.CountryName)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View(countries);
        }
    }
}
