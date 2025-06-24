using AsiaGuides.Data;
using AsiaGuides.Models;
using AsiaGuides.Utility;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;

namespace AsiaGuides.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CountryController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CountryController> _logger;
        private readonly Cloudinary _cloudinary;

        public CountryController(ApplicationDbContext dbContext, ILogger<CountryController> logger, Cloudinary cloudinary)
        {
            _dbContext = dbContext;
            _logger = logger;
            _cloudinary = cloudinary;
        }

        public async Task<IActionResult> Index()
        {
            var account = new Account("dftflczf1", "613875171863164", "uhe28k9WIl8p1YTSc6wlaVRKoPI");
            var cloudinary = new Cloudinary(account);

            List<string> publicIds = new List<string>()
            {
                "ngsir8v9lh2ecjg7hfbi",
                "enxescglqqkvumhshlex",
                "ihg1pff779lzsmczs40y",
                "ihg1pff779lzsmczs40y"
            };

            foreach (var item in publicIds)
            {
                try
                {

                    var result = cloudinary.GetResource(new GetResourceParams(item));
                    if (result == null || result.SecureUrl == null)
                    {
                        ViewBag.Message = $"File is not found";
                    }
                    else
                    {
                        ViewBag.Message = $"File {result.SecureUrl} exists";
                    }
                }
                catch(Exception ex)
                {
                    ViewBag.Message = $"Error: {ex.Message}";
                }

            }

            var countries = await _dbContext.Countries.ToListAsync();
            if (!countries.Any())
            {
                ViewBag.Message = "There are no countries in the list yet";
                return View("Empty");
            }
            return View(countries);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var country = await _dbContext.Countries
                .Include(c => c.Cities)
                .FirstOrDefaultAsync(c => c.Id == id);

            return country == null ? NotFound() : View(country);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Country country, IFormFile file)
        {
            if (!ModelState.IsValid) return View(country);

            country.ImageUrl = await UploadImageAsync(file) ?? "/images/empty.png";

            _dbContext.Countries.Add(country);
            await _dbContext.SaveChangesAsync();

            TempData["success"] = "The country has been created successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue) return NotFound();

            var country = await _dbContext.Countries.FindAsync(id);
            return country == null ? NotFound() : View(country);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Country country, IFormFile file)
        {
            if (!ModelState.IsValid) return View(country);

            var countryFromDb = await _dbContext.Countries.FindAsync(country.Id);
            if (countryFromDb == null) return NotFound();

            if (file != null && file.Length > 0)
            {
                country.ImageUrl = await UploadImageAsync(file);
            }
            else if (string.IsNullOrEmpty(country.ImageUrl))
            {
                country.ImageUrl = "/images/empty.png";
            }

            _dbContext.Entry(countryFromDb).State = EntityState.Detached;
            _dbContext.Countries.Update(country);
            await _dbContext.SaveChangesAsync();

            TempData["success"] = "The country has been edited successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue) return NotFound();

            var country = await _dbContext.Countries.FindAsync(id);
            return country == null ? NotFound() : View(country);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var country = await _dbContext.Countries.FindAsync(id);
            if (country == null) return NotFound();

            _dbContext.Countries.Remove(country);
            await _dbContext.SaveChangesAsync();

            TempData["success"] = "The country has been deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int countryId)
        {
            var country = await _dbContext.Countries.FindAsync(countryId);
            if (country == null || string.IsNullOrEmpty(country.ImageUrl)) return NotFound();

            country.ImageUrl = null;
            _dbContext.Countries.Update(country);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        private async Task<string?> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Transformation = new Transformation().Width(500).Height(500).Crop("fill")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            return uploadResult.StatusCode == System.Net.HttpStatusCode.OK
                ? uploadResult.SecureUrl.ToString()
                : null;
        }
    }
}
