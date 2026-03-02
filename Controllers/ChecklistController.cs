using Microsoft.AspNetCore.Mvc;
using travel_dost_asp.net.Data;
using travel_dost_asp.net.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace travel_dost_asp.net.Controllers
{
    public class ChecklistController : Controller
    {
        private readonly TravelDostContext _context;

        public ChecklistController(TravelDostContext context)
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

        public async Task<IActionResult> Index(int? trip_id)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var trips = await _context.Trips
                .Where(t => t.UserId == userId)
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

            var checklistQuery = _context.ChecklistItems
                .Join(_context.CustomChecklists, 
                      item => item.ChecklistId, 
                      list => list.Id, 
                      (item, list) => new { item, list })
                .Where(x => x.list.UserId == userId);

            if (trip_id != null)
            {
                checklistQuery = checklistQuery.Where(x => x.list.TripId == trip_id);
            }
            else
            {
                checklistQuery = checklistQuery.Where(x => x.list.TripId == null);
            }

            var items = await checklistQuery
                .Select(x => x.item)
                .OrderBy(i => i.Id)
                .ToListAsync();

            ViewBag.Trips = trips;
            ViewBag.SelectedTripId = trip_id;
            
            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Add(string itemName, int? tripId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // Find or create the checklist for this trip/user
            var checklist = await _context.CustomChecklists
                .FirstOrDefaultAsync(c => c.UserId == userId && c.TripId == tripId);

            if (checklist == null)
            {
                checklist = new CustomChecklist { UserId = userId.Value, TripId = tripId };
                _context.CustomChecklists.Add(checklist);
                await _context.SaveChangesAsync();
            }

            var item = new ChecklistItem
            {
                ChecklistId = checklist.Id,
                ItemName = itemName,
                IsChecked = false,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ChecklistItems.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { trip_id = tripId });
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var item = await _context.ChecklistItems.FindAsync(id);
            if (item == null) return NotFound();

            item.IsChecked = !item.IsChecked;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.ChecklistItems.FindAsync(id);
            if (item == null) return NotFound();

            _context.ChecklistItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
