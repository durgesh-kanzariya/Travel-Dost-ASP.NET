using Microsoft.AspNetCore.Mvc;
using travel_dost_asp.net.Data;
using travel_dost_asp.net.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace travel_dost_asp.net.Controllers
{
    public class TripsController : Controller
    {
        private readonly TravelDostContext _context;

        public TripsController(TravelDostContext context)
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

            ViewBag.Countries = await _context.CountryGuides
                .Select(c => c.CountryName)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Project NotMapped Destination field
            var trips = await _context.Trips
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.StartDate)
                .Select(t => new Trip {
                    TripId = t.TripId,
                    UserId = t.UserId,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Budget = t.Budget,
                    Currency = t.Currency,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Destination = string.Join("|", _context.TripDestinations
                        .Where(td => td.TripId == t.TripId)
                        .OrderBy(td => td.VisitOrder)
                        .Join(_context.Destinations, td => td.DestinationId, d => d.Id, (td, d) => d.LocationName))
                })
                .ToListAsync();

            return View(trips);
        }

        [HttpPost]
        public async Task<IActionResult> Create(List<string> destinations, DateTime startDate, DateTime endDate, decimal budget, string currency)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var trip = new Trip
            {
                UserId = userId.Value,
                StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc),
                Budget = budget,
                Currency = currency ?? "USD",
                Status = "Planned",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            if (destinations != null && destinations.Any())
            {
                var validDests = destinations.Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => d.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                for (int i = 0; i < validDests.Count; i++)
                {
                    var destName = validDests[i];
                    var dest = await _context.Destinations.FirstOrDefaultAsync(d => d.LocationName.ToLower() == destName.ToLower());
                    if (dest == null)
                    {
                        dest = new Destination { LocationName = destName, CountryName = "Unknown", LocationType = "city" };
                        _context.Destinations.Add(dest);
                        await _context.SaveChangesAsync();
                    }

                    _context.TripDestinations.Add(new TripDestination { TripId = trip.TripId, DestinationId = dest.Id, VisitOrder = i + 1 });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, List<string> destinations, DateTime startDate, DateTime endDate, decimal budget, string currency)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var trip = await _context.Trips.FindAsync(id);
            if (trip == null || trip.UserId != userId) return NotFound();

            trip.StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            trip.EndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            trip.Budget = budget;
            trip.Currency = currency;
            trip.UpdatedAt = DateTime.UtcNow;

            // Handle destination update
            if (destinations != null && destinations.Any())
            {
                var existingMaps = _context.TripDestinations.Where(td => td.TripId == id);
                _context.TripDestinations.RemoveRange(existingMaps);
                
                var validDests = destinations.Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => d.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                for(int i = 0; i < validDests.Count; i++)
                {
                    var destName = validDests[i];
                    var dest = await _context.Destinations.FirstOrDefaultAsync(d => d.LocationName.ToLower() == destName.ToLower());
                    if (dest == null)
                    {
                        dest = new Destination { LocationName = destName, CountryName = "Unknown", LocationType = "city" };
                        _context.Destinations.Add(dest);
                        await _context.SaveChangesAsync();
                    }
                    _context.TripDestinations.Add(new TripDestination { TripId = id, DestinationId = dest.Id, VisitOrder = i + 1 });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var trip = await _context.Trips.FindAsync(id);
            if (trip == null || trip.UserId != userId) return NotFound();

            // Handle cascading if needed (checklists, expenses)
            var expenses = _context.Expenses.Where(e => e.TripId == id);
            _context.Expenses.RemoveRange(expenses);

            var checklists = _context.CustomChecklists.Where(c => c.TripId == id);
            foreach(var cl in checklists) {
                var items = _context.ChecklistItems.Where(i => i.ChecklistId == cl.Id);
                _context.ChecklistItems.RemoveRange(items);
            }
            _context.CustomChecklists.RemoveRange(checklists);

            var tripDestinations = _context.TripDestinations.Where(td => td.TripId == id);
            _context.TripDestinations.RemoveRange(tripDestinations);

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
