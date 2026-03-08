using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http;

namespace travel_dost_asp.net.Controllers
{
    public class TranslatorController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TranslatorController(IHttpClientFactory httpClientFactory)
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

        [HttpGet("api/translate")]
        public async Task<IActionResult> Translate(string text, string from, string to)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                return BadRequest("Missing parameters: text, from, to");
            }

            var client = _httpClientFactory.CreateClient();
            
            // Try Google Translate internal API
            try
            {
                var googleUrl = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={from}&tl={to}&dt=t&q={Uri.EscapeDataString(text)}";
                var response = await client.GetAsync(googleUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    var firstArray = doc.RootElement[0];
                    string translatedText = "";
                    foreach (var part in firstArray.EnumerateArray())
                    {
                        translatedText += part[0].GetString();
                    }

                    if (!string.IsNullOrEmpty(translatedText))
                    {
                        return Ok(new { translatedText, match = 0.95 });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error potentially
            }

            // Fallback to MyMemory
            try
            {
                var myMemoryUrl = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair={from}|{to}";
                var response = await client.GetAsync(myMemoryUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    var responseData = doc.RootElement.GetProperty("responseData");
                    string translatedText = responseData.GetProperty("translatedText").GetString();
                    double match = doc.RootElement.GetProperty("match").GetDouble();

                    if (!string.IsNullOrEmpty(translatedText))
                    {
                        return Ok(new { translatedText, match });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
            }

            return StatusCode(500, "Translation service unavailable");
        }
    }
}
