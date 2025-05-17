using AsiaGuides.Data;
using AsiaGuides.Models;
using AsiaGuides.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsiaGuides.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = StaticDetails.Role_User)]
    [AllowAnonymous]
    public class UserHomeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        public UserHomeController(ApplicationDbContext _dbContext)
        {
            this._dbContext = _dbContext;
        }

        /// <summary>
        /// метод для вызова списка стран
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            IEnumerable<Country> countries = await _dbContext.Countries.ToListAsync();
            return View(countries);
        }

        public async Task<IActionResult> CountryDetails(int? id)
        {
            if (!id.HasValue) return NotFound();
            Country country = await _dbContext.Countries.Include(c => c.Cities).FirstOrDefaultAsync(c => c.Id.Equals(id.Value));
            if (country == null) return NotFound();
            return View(country);
        }

        public async Task<IActionResult> GetCities()
        {
            IEnumerable<City> cities = await _dbContext.Cities.ToListAsync();
            return View(cities);
        }

        public async Task<IActionResult> CityyDetails(int? id)
        {
            if (!id.HasValue) return NotFound();
            City city = await _dbContext.Cities.Include(c => c.Attractions).ThenInclude(a => a.Images).FirstOrDefaultAsync(c => c.Id == id.Value);
            if (city == null) return NotFound();
            return View(city);
        }


        public async Task<IActionResult> GetAttractions()
        {
            IEnumerable<Attraction> attractions = await _dbContext.Attractions.Include(a => a.Images).ToListAsync();
            return View(attractions);
        }

        public async Task<IActionResult> AttractionDetails(int? id)
        {
            if (!id.HasValue) return NotFound();
            Attraction attraction = await _dbContext.Attractions.Include(a => a.Images).FirstOrDefaultAsync(a => a.Id.Equals(id.Value));
            if (attraction == null) return NotFound();
            return View(attraction);
        }
    }
}
