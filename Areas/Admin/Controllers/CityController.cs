using AsiaGuides.Data;
using AsiaGuides.Models;
using AsiaGuides.Utility;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AsiaGuides.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CityController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CityController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly Cloudinary _cloudinary;

        public CityController(ApplicationDbContext dbContext, IWebHostEnvironment env, ILogger<CityController> logger, Cloudinary cloudinary)
        {
            _dbContext = dbContext;
            _env = env;
            _logger = logger;
            _cloudinary = cloudinary;
        }

        public async Task<IActionResult> Index()
        {
            var cities = await _dbContext.Cities.ToListAsync();
            if (!cities.Any())
            {
                ViewBag.Message = "There are no cities in the list yet";
                return View("Empty");
            }
            return View(cities);
        }

        public async Task<IActionResult> Details(int id)
        {
            var city = await _dbContext.Cities
                .Include(c => c.Attractions)
                    .ThenInclude(a => a.Images)
                .FirstOrDefaultAsync(c => c.Id == id);
            return city == null ? NotFound() : View(city);
        }

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

            if (file != null)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    city.ImageUrl = uploadResult.SecureUrl.ToString();
                else
                {
                    ModelState.AddModelError(string.Empty, "Image upload failed.");
                    return View(city);
                }
            }
            else
            {
                city.ImageUrl = "/images/empty.png";
            }

            await _dbContext.Cities.AddAsync(city);
            var country = await _dbContext.Countries.Include(c => c.Cities).FirstOrDefaultAsync(c => c.Id == city.CountryId);
            country?.Cities.Add(city);
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "The city has been created successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var city = await _dbContext.Cities.FindAsync(id);
            if (city == null) return NotFound();
            ViewBag.CountryList = new SelectList(await _dbContext.Countries.ToListAsync(), "Id", "Name");
            return View(city);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(City city, IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CountryList = new SelectList(await _dbContext.Countries.ToListAsync(), "Id", "Name");
                return View(city);
            }

            var cityFromDb = await _dbContext.Cities.FindAsync(city.Id);
            if (cityFromDb == null) return NotFound();

            cityFromDb.Name = city.Name;
            cityFromDb.Description = city.Description;
            cityFromDb.CountryId = city.CountryId;

            if (file != null)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    cityFromDb.ImageUrl = uploadResult.SecureUrl.ToString();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Image upload failed.");
                    return View(city);
                }
            }

            if (string.IsNullOrEmpty(cityFromDb.ImageUrl))
                cityFromDb.ImageUrl = "/images/empty.png";

            await _dbContext.SaveChangesAsync();
            TempData["success"] = "The city has been edited successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var city = await _dbContext.Cities.FindAsync(id);
            return city == null ? NotFound() : View(city);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int? id)
        {
            if (id == null) return NotFound();
            var city = await _dbContext.Cities.FindAsync(id);
            if (city == null) return NotFound();

            _dbContext.Cities.Remove(city);
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "The city has been deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int? cityId)
        {
            var city = await _dbContext.Cities.FindAsync(cityId);
            if (city == null || string.IsNullOrEmpty(city.ImageUrl)) return NotFound();

            // Локальное удаление возможно только если путь относительный
            if (!city.ImageUrl.StartsWith("http"))
            {
                var imagePath = Path.Combine(_env.WebRootPath, city.ImageUrl.TrimStart('/'));
                try
                {
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error deleting image: {ex.Message}");
                }
            }

            city.ImageUrl = null;
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
