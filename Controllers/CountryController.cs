using AsiaGuides.Data;
using AsiaGuides.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AsiaGuides.Controllers
{
    public class CountryController : Controller
    {
        private ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<CountryController> _logger;
        public CountryController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment, ILogger<CountryController> logger)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<Country> countriesFromDb = await _dbContext.Countries.ToListAsync();
            if (!countriesFromDb.Any())
            {
                ViewBag.Message = "There are no countries in the list yet";
                return View("Empty");
            }
            return View(countriesFromDb);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id == 0) return NotFound(); // Проверка на null
            Country? countryFromDb = await _dbContext.Countries.Include(c => c.Cities).FirstOrDefaultAsync(c => c.Id == id); // Используем id.Value
            if (countryFromDb == null) return NotFound(); // Если country не найдено
            return View(countryFromDb);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Country country, IFormFile file)
        {
            if (!ModelState.IsValid) return View();

            string wwwRootPath = _webHostEnvironment.WebRootPath;

            if (file != null && file.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string countryPath = Path.Combine(wwwRootPath, "images");

                using (var fileStream = new FileStream(Path.Combine(countryPath, fileName), FileMode.Create))
                {
                   await file.CopyToAsync(fileStream);
                }

                country.ImageUrl = "/images/" + fileName;
            }
            else
            {
                country.ImageUrl = "/images/empty.png";
            }

            await _dbContext.Countries.AddAsync(country);
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "The country has been created successfully";
            return RedirectToAction(nameof(Index), "Country");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue) return NotFound();
            Country countryFromDb = await _dbContext.Countries.FindAsync(id); // Используем id.Value
            if (countryFromDb == null) return NotFound(); // Если country не найдено
            return View(countryFromDb);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Country country, IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                return View(country); // Возвращаем модель с ошибками валидации
            }

            // Получаем путь для сохранения изображения
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            // Проверяем, если есть файл, то загружаем его
            if (file != null && file.Length > 0)
            {
                // Генерируем уникальное имя для файла
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string countryPath = Path.Combine(wwwRootPath, "images");

                // Создаем файл в папке "images"
                using (var fileStream = new FileStream(Path.Combine(countryPath, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream); // Асинхронное копирование
                }

                country.ImageUrl = "/images/" + fileName; // Обновляем путь изображения
            }
            else if (string.IsNullOrEmpty(country.ImageUrl)) // Если не был выбран файл, ставим дефолтное изображение
            {
                country.ImageUrl = "/images/empty.png"; // Дефолтное изображение для страны
            }

            // Проверяем существует ли страна в базе данных перед обновлением
            var countryFromDb = await _dbContext.Countries.FindAsync(country.Id);
            if (countryFromDb == null)
            {
                return NotFound(); // Если страна не найдена, возвращаем ошибку 404
            }

            var trackedCountry = _dbContext.Countries.Local.FirstOrDefault(c => c.Id == country.Id);
            if (trackedCountry != null)
            {
                _dbContext.Entry(trackedCountry).State = EntityState.Detached;  // Отключаем отслеживание старой сущности
            }

            // Обновляем информацию о стране в базе данных
            _dbContext.Countries.Update(country);
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "The country has been edited successfully";
            return RedirectToAction(nameof(Index), "Country"); // Перенаправляем на страницу списка стран
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue) return NotFound();
            Country countryToBeDeleted = await _dbContext.Countries.FindAsync(id);
            if (countryToBeDeleted == null) return NotFound();
            return View(countryToBeDeleted);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletPost(int id)
        {
            Country countryToBeDeleted = await _dbContext.Countries.FindAsync(id);
            if (countryToBeDeleted == null) return NotFound();

            if(!string.IsNullOrEmpty(countryToBeDeleted.ImageUrl))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, countryToBeDeleted.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
            }

            _dbContext.Countries.Remove(countryToBeDeleted);
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "The country has been deleted successfully";
            return RedirectToAction(nameof(Index), "Country");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int countryId)
        {
            Country country = await _dbContext.Countries.FindAsync(countryId);
            if(country == null) return NotFound();
            string imageUrl = country.ImageUrl;
            if (string.IsNullOrEmpty(imageUrl)) return NotFound();
            // Удаляем физический файл
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('\\'));
            try
            {
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                else
                {
                    _logger.LogWarning($"The file {oldImagePath} does not exist.", oldImagePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error: {ex.Message}");
            }
            _dbContext.Countries.Update(country);
            country.ImageUrl = null;
            await _dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK);
        }
    }
}
