using Microsoft.AspNetCore.Mvc;
using travel_dost_asp.net.Data;
using travel_dost_asp.net.Models;
using Microsoft.EntityFrameworkCore;

namespace travel_dost_asp.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuideController : ControllerBase
    {
        private readonly TravelDostContext _context;

        public GuideController(TravelDostContext context)
        {
            _context = context;
        }

        [HttpGet("{country}")]
        public async Task<IActionResult> GetGuide(string country)
        {
            var guide = await _context.CountryGuides
                .FirstOrDefaultAsync(g => g.CountryName.ToLower() == country.ToLower());

            if (guide == null)
            {
                return Ok(new
                {
                    countryName = country,
                    policeNumber = "112",
                    ambulanceNumber = "112",
                    fireNumber = "112",
                    localRules = "Respect local laws and customs.",
                    isFallback = true
                });
            }

            return Ok(guide);
        }

        [HttpGet("countries")]
        public async Task<IActionResult> GetAllCountries()
        {
            var countries = await _context.CountryGuides
                .Select(g => g.CountryName)
                .Distinct()
                .ToListAsync();
            return Ok(countries);
        }
    }
}
