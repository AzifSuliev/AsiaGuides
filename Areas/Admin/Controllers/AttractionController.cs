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
    public class AttractionController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Cloudinary _cloudinary;

        public AttractionController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment, Cloudinary cloudinary)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _cloudinary = cloudinary;
        }

        public async Task<IActionResult> Index()
        {
            var attractions = await _dbContext.Attractions.Include(a => a.Images).ToListAsync();
            if (!attractions.Any())
            {
                ViewBag.Message = "There are no attractions in the list yet";
                return View("Empty");
            }
            return View(attractions);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var attraction = await _dbContext.Attractions
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            return attraction == null ? NotFound() : View(attraction);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.CityList = new SelectList(await _dbContext.Cities.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Attraction attraction, List<IFormFile> files)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CityList = new SelectList(await _dbContext.Cities.ToListAsync(), "Id", "Name");
                return View(attraction);
            }

            await _dbContext.Attractions.AddAsync(attraction);
            await _dbContext.SaveChangesAsync();

            await UploadImagesToCloudinary(attraction, files);
            await _dbContext.SaveChangesAsync();

            TempData["success"] = "The attraction has been created successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var attraction = await _dbContext.Attractions
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attraction == null) return NotFound();

            ViewBag.CityList = new SelectList(await _dbContext.Cities.ToListAsync(), "Id", "Name");
            return View(attraction);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Attraction attraction, List<IFormFile> files)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CityList = new SelectList(await _dbContext.Cities.ToListAsync(), "Id", "Name");
                return View(attraction);
            }

            var attractionFromDb = await _dbContext.Attractions
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == attraction.Id);

            if (attractionFromDb == null) return NotFound();

            attractionFromDb.Name = attraction.Name;
            attractionFromDb.Description = attraction.Description;
            attractionFromDb.Rating = attraction.Rating;
            attractionFromDb.OpeningHours = attraction.OpeningHours;
            attractionFromDb.CityId = attraction.CityId;

            await UploadImagesToCloudinary(attractionFromDb, files);
            await _dbContext.SaveChangesAsync();

            TempData["success"] = "The attraction has been edited successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var attraction = await _dbContext.Attractions.FirstOrDefaultAsync(a => a.Id == id);
            return attraction == null ? NotFound() : View(attraction);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int? id)
        {
            if (id == null) return NotFound();

            var attraction = await _dbContext.Attractions
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attraction == null) return NotFound();

            foreach (var image in attraction.Images)
            {
                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    var deletionParams = new DeletionParams(image.PublicId);
                    await _cloudinary.DestroyAsync(deletionParams);
                }
            }

            _dbContext.Attractions.Remove(attraction);
            await _dbContext.SaveChangesAsync();

            TempData["success"] = "The attraction has been deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int attractionId, int imageId)
        {
            var attraction = await _dbContext.Attractions
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == attractionId);

            if (attraction == null) return NotFound();

            var image = attraction.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null) return NotFound();
            if (!string.IsNullOrEmpty(image.PublicId))
            {
                var deletionParams = new DeletionParams(image.PublicId);
                await _cloudinary.DestroyAsync(deletionParams);
            }

            attraction.Images.Remove(image);
            await _dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK);
        }

        private async Task UploadImagesToCloudinary(Attraction attraction, List<IFormFile> files)
        {
            if (files == null || files.Count == 0) return;

            foreach (var file in files)
            {
                if (file?.Length > 0)
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, file.OpenReadStream()),
                        Transformation = new Transformation().Width(800).Height(600).Crop("fill")
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        attraction.Images.Add(new AttractionImage
                        {
                            ImageUrl = uploadResult.SecureUrl.ToString(),
                            PublicId = uploadResult.PublicId,
                            AttractionId = attraction.Id
                        });
                    }
                }
            }
        }
    }
}
