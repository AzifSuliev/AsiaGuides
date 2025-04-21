using AsiaGuides.Data;
using AsiaGuides.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AsiaGuides.Controllers
{
    public class AttractionController : Controller
    {
        private ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AttractionController(ApplicationDbContext _dbContext, IWebHostEnvironment _webHostEnvironment)
        {
            this._dbContext = _dbContext;
            this._webHostEnvironment = _webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Attraction> attractions = await _dbContext.Attractions.Include(a => a.Images).ToListAsync();
            if (!attractions.Any())
            {
                ViewBag.Message = "There are no attractions in the list yet";
                return View("Empty");
            }
            return View(attractions);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue) return NotFound();
            Attraction attractionFromDb = await _dbContext.Attractions.Include(a => a.Images).FirstOrDefaultAsync(a => a.Id == id);
            if (attractionFromDb == null) return NotFound();
            return View(attractionFromDb);
        }

        [HttpGet]
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
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string attractionPath = Path.Combine(wwwRootPath, "images");
                    if (file != null && file.Length > 0)
                    {
                        // Название файла
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                        using (var fileStream = new FileStream(Path.Combine(attractionPath, fileName), FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        var attractionImage = new AttractionImage()
                        {
                            ImageUrl = "/images/" + fileName,
                            AttractionId = attraction.Id
                        };
                        attraction.Images.Add(attractionImage);
                    }
                }
                await _dbContext.SaveChangesAsync();
            }

            TempData["success"] = "The attraction has been created successfully";
            return RedirectToAction(nameof(Index), "Attraction");
        }
    }
}
