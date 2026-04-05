using Microsoft.AspNetCore.Mvc;
using travel_dost_asp.net.Data;
using travel_dost_asp.net.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace travel_dost_asp.net.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly TravelDostContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public ExpensesController(TravelDostContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
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

            if (trip_id == null && trips.Any())
            {
                trip_id = trips.First().TripId;
            }

            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.TripId == trip_id)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();

            var trip = trips.FirstOrDefault(t => t.TripId == trip_id);
            decimal totalSpent = 0;

            if (trip != null && expenses.Any())
            {
                var baseCurrency = trip.Currency ?? "USD";
                var client = _httpClientFactory.CreateClient();
                try
                {
                    var response = await client.GetAsync($"https://api.exchangerate-api.com/v4/latest/{baseCurrency}");
                    var normalizedAmounts = new Dictionary<int, decimal>();
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(content);
                        var rates = doc.RootElement.GetProperty("rates");

                        foreach (var e in expenses)
                        {
                            var expCurrency = e.Currency ?? baseCurrency;
                            if (expCurrency == baseCurrency)
                            {
                                totalSpent += e.Amount;
                            }
                            else if (rates.TryGetProperty(expCurrency, out var rateElement))
                            {
                                var rate = rateElement.GetDecimal();
                                if (rate > 0)
                                {
                                    var normAmount = e.Amount / rate;
                                    totalSpent += normAmount;
                                    normalizedAmounts[e.Id] = normAmount;
                                }
                            }
                            else
                            {
                                totalSpent += e.Amount; // Fallback
                            }
                        }
                    }
                    else
                    {
                        totalSpent = expenses.Sum(e => e.Amount); // Fallback
                    }
                    
                    ViewBag.NormalizedAmounts = normalizedAmounts;
                }
                catch
                {
                    totalSpent = expenses.Sum(e => e.Amount); // Fallback
                }
            }

            ViewBag.Trips = trips;
            ViewBag.SelectedTrip = trip;
            ViewBag.TotalSpent = totalSpent;
            ViewBag.Budget = trip?.Budget ?? 0;

            return View(expenses);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int tripId, decimal amount, string category, string description, DateTime expenseDate, string currency)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var expense = new Expense
            {
                TripId = tripId,
                UserId = userId.Value,
                Amount = amount,
                Category = category,
                Description = description,
                ExpenseDate = expenseDate == default ? DateTime.UtcNow : DateTime.SpecifyKind(expenseDate, DateTimeKind.Utc),
                Currency = string.IsNullOrEmpty(currency) ? ((await _context.Trips.FindAsync(tripId))?.Currency ?? "USD") : currency
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { trip_id = tripId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            var tripId = expense.TripId;
            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return Ok(new { tripId });
        }
    }
}
