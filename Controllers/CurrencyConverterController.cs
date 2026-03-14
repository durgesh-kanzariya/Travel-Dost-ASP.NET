using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;

namespace travel_dost_asp.net.Controllers
{
    public class CurrencyConverterController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CurrencyConverterController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("user")))
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        [HttpGet("api/currency/rates/{baseCode}")]
        public async Task<IActionResult> GetRates(string baseCode)
        {
            var client = _httpClientFactory.CreateClient();
            try
            {
                var response = await client.GetAsync($"https://api.exchangerate-api.com/v4/latest/{baseCode}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }
                return StatusCode((int)response.StatusCode, "Failed to fetch rates");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
