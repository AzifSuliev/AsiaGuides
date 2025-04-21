using AsiaGuides.Data;
using AsiaGuides.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

namespace AsiaGuides.Controllers
{
    public class CityController : Controller
    {
        private  ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CityController(ApplicationDbContext _dbContext, IWebHostEnvironment _webHostEnvironment)
        {
            this._dbContext = _dbContext;
            this._webHostEnvironment = _webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<City> citiesFromDb = await _dbContext.Cities.ToListAsync();
            if (!citiesFromDb.Any())
            {
                ViewBag.Message = "There are no cities in the list yet";
                return View("Empty");
            }
            return View(citiesFromDb);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id == null || id == 0) return NotFound();
            City cityFromDb = await _dbContext.Cities.Include(c => c.Attractions).ThenInclude(a => a.Images).FirstOrDefaultAsync(c => c.Id == id);
            if (cityFromDb == null) return NotFound();
            return View(cityFromDb);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.CountryList = new SelectList(await _dbContext.Countries.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(City city, IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CountryList = new SelectList(await _dbContext.Countries.ToListAsync(), "Id", "Name");
                return View(city);
            }
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            if (file != null && file.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string cityPath = Path.Combine(wwwRootPath, "images");
                using (var fileStream = new FileStream(Path.Combine(cityPath, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                city.ImageUrl = "/images/" + fileName;
            }
            else city.ImageUrl = "/images/empty.png";

            await _dbContext.Cities.AddAsync(city);  // Добавляем новый город в базу данных

            // Загружаем страну, к которой относится город
            var country = await _dbContext.Countries.
                Include(c => c.Cities).
                FirstOrDefaultAsync(c => c.Id == city.CountryId);

            // Добавляем новый город в коллекцию Cities страны
            country?.Cities.Add(city); // Если country != null, то добавить city в Cities
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "The city has been created successfully";
            return RedirectToAction(nameof(Index), "City");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue) return NotFound();
            City cityFromDb = await _dbContext.Cities.FindAsync(id.Value);
            if (cityFromDb == null) return NotFound();
            ViewBag.CountryList = new SelectList(await _dbContext.Countries.ToListAsync(), "Id", "Name");
            return View(cityFromDb);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(City city, IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CountryList = new SelectList(await _dbContext.Countries.ToListAsync(), "Id", "Name");
                return View(city);
            }

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null && file.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string cityPath = Path.Combine(wwwRootPath, "images");
                using (var fileStream = new FileStream(Path.Combine(cityPath, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                city.ImageUrl = "/images/" + fileName;
            }
            else if(string.IsNullOrEmpty(city.ImageUrl)) city.ImageUrl = "/images/empty.png";

            var cityFromDb = await _dbContext.Cities.FindAsync(city.Id);
            if (cityFromDb == null) return NotFound();
            var trackedCity = _dbContext.Cities.Local.FirstOrDefault(c => c.Id == city.Id);
            if (trackedCity != null)
            {
                _dbContext.Entry(trackedCity).State = EntityState.Detached;  // Отключаем отслеживание старой сущности
            }

            _dbContext.Cities.Update(city);
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "The city has been edited successfully";
            return RedirectToAction(nameof(Index), "City");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue) return NotFound();
            City cityToBeDeleted = await _dbContext.Cities.FindAsync(id.Value);
            if (cityToBeDeleted == null) return NotFound();
            return View(cityToBeDeleted);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int? id)
        {
            if (!id.HasValue) return NotFound();
            City cityToBeDeleted = await _dbContext.Cities.FindAsync(id.Value);
            if (cityToBeDeleted == null) return NotFound();

            if (string.IsNullOrEmpty(cityToBeDeleted.ImageUrl))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, cityToBeDeleted.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
            }

            _dbContext.Cities.Remove(cityToBeDeleted);
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "The city has been deleted successfully";
            return RedirectToAction(nameof(Index), "City");
        }
    }
}
