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
            // Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                // Если модель некорректна, возвращаем список городов для выпадающего списка и возвращаем форму обратно
                ViewBag.CityList = new SelectList(await _dbContext.Cities.ToListAsync(), "Id", "Name");
                return View(attraction);
            }
            // Сначала сохраняем объект Attraction в базу данных (без изображений)
            await _dbContext.Attractions.AddAsync(attraction);
            await _dbContext.SaveChangesAsync(); // Это нужно, чтобы attraction.Id стал доступен
            // Если были загружены изображения
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    // Путь к wwwroot
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    // Путь к папке с изображениями
                    string attractionPath = Path.Combine(wwwRootPath, "images");
                    // Если файл загружен
                    if (file != null && file.Length > 0)
                    {
                        // Генерируем уникальное имя файла
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        // Сохраняем файл на диск 
                        using (var fileStream = new FileStream(Path.Combine(attractionPath, fileName), FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        // Создаём объект изображения и связываем его с достопримечательностью
                        var attractionImage = new AttractionImage()
                        {
                            ImageUrl = "/images/" + fileName,
                            AttractionId = attraction.Id
                        };
                        // Добавляем изображение в коллекцию
                        attraction.Images.Add(attractionImage);
                    }
                }
                // Сохраняем изображения в базу данных
                await _dbContext.SaveChangesAsync();
            }
            // Успешное сообщение
            TempData["success"] = "The attraction has been created successfully";
            // Перенаправление на список достопримечательностей
            return RedirectToAction(nameof(Index), "Attraction");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue) return NotFound();
            Attraction attractionFromDb = await _dbContext.Attractions.Include(a => a.Images).FirstOrDefaultAsync(a => a.Id == id.Value);
            if (attractionFromDb == null) return NotFound();
            // Список городов для выбора
            ViewBag.CityList = new SelectList(await _dbContext.Cities.ToListAsync(), "Id", "Name");
            return View(attractionFromDb);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Attraction attraction, List<IFormFile> files)
        {
            // Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                // Если модель некорректна, возвращаем список городов для выпадающего списка и возвращаем форму обратно
                ViewBag.CityList = new SelectList(await _dbContext.Cities.ToListAsync(), "Id", "Name");
                return View(attraction);
            }
            // Загружаем старую достопримечательность из базы вместе с изображениями
            var attractionFromDb = await _dbContext.Attractions.Include(a => a.Images).FirstOrDefaultAsync(a => a.Id == attraction.Id);

            if (attractionFromDb == null) return NotFound();

            // Обновляем свойства
            attractionFromDb.Name = attraction.Name;
            attractionFromDb.Description = attraction.Description;
            attractionFromDb.Rating = attraction.Rating;
            attractionFromDb.OpeningHours = attraction.OpeningHours;
            attractionFromDb.CityId = attraction.CityId;


            // Если были загружены изображения
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    // Путь к папке wwwroot
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    // Путь к папке с изображениями
                    string attractionPath = Path.Combine(wwwRootPath, "images");
                    // Если файл загружен 
                    if (file != null && file.Length > 0)
                    {
                        // Генерируем уникальное имя файла
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        // Сохраняем файл на диск 
                        using (var fileStream = new FileStream(Path.Combine(attractionPath, fileName), FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        // Создаём объект изображения и связываем его с достопримечательностью
                        var attractionImage = new AttractionImage()
                        {
                            ImageUrl = "/images/" + fileName,
                            AttractionId = attraction.Id
                        };
                        // Добавляем изображение в коллекцию
                        attractionFromDb.Images.Add(attractionImage);
                    }
                }
            }
            // Сохраняем изображения в базу данных
            await _dbContext.SaveChangesAsync();
            // Успешное сообщение
            TempData["success"] = "The attraction has been edited successfully";
            // Перенаправление на список достопримечательностей
            return RedirectToAction(nameof(Index), "Attraction");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue) return NotFound();
            Attraction attraction = await _dbContext.Attractions.FirstOrDefaultAsync(a => a.Id == id.Value);
            if (attraction == null) return NotFound();
            return View(attraction);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int? id)
        {
            if (!id.HasValue) return NotFound();
            Attraction attractionToBeDeleted = await _dbContext.Attractions.Include(a => a.Images).FirstOrDefaultAsync(a => a.Id == id.Value);
            if (attractionToBeDeleted == null) return NotFound();
            foreach (var image in attractionToBeDeleted.Images)
            {
                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                }
            }
            _dbContext.Attractions.Remove(attractionToBeDeleted);
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "The attraction has been deleted successfully";
            return RedirectToAction(nameof(Index), "Attraction");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int attractionId, int imageId)
        {
            var attraction = await _dbContext.Attractions.Include(a => a.Images).FirstOrDefaultAsync(a => a.Id == attractionId);
            if (attraction == null) return NotFound();
            var image = attraction.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null) return NotFound();
            // Удаляем физический файл
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            attraction.Images.Remove(image);
            await _dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK);
        }
    }
}
