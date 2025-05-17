using AsiaGuides.Data;
using AsiaGuides.Models;
using AsiaGuides.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using static System.Net.Mime.MediaTypeNames;

namespace AsiaGuides.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CityController : Controller
    {
        private ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<CityController> _logger;
        public CityController(ApplicationDbContext _dbContext, IWebHostEnvironment _webHostEnvironment, ILogger<CityController> _logger)
        {
            this._dbContext = _dbContext;
            this._webHostEnvironment = _webHostEnvironment;
            this._logger = _logger;
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
            // Загружаем список стран из базы данных и передаём его во ViewBag для отображения в выпадающем списке
            ViewBag.CountryList = new SelectList(await _dbContext.Countries.ToListAsync(), "Id", "Name");
            // Отображение формы создания сущности
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(City city, IFormFile file)
        {
            // Если модель некорректна, возвращаем пользователя на форму с ошибками
            if (!ModelState.IsValid)
            {
                // Повторно загружаем список стран для формы
                ViewBag.CountryList = new SelectList(await _dbContext.Countries.ToListAsync(), "Id", "Name");
                // Возвращаем обратно форму с введёнными данными
                return View(city);
            }

            // Получаем путь к папке wwwroot
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            // Если пользователь загрузил файл изображения
            if (file != null && file.Length > 0)
            {
                // Генерируем уникальное имя файла с сохранением расширения
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                // Путь к папке "images"
                string cityPath = Path.Combine(wwwRootPath, "images");
                // Сохраняем файл на диск
                using (var fileStream = new FileStream(Path.Combine(cityPath, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                // Указываем путь к изображению в свойство города
                city.ImageUrl = "/images/" + fileName;
            }
            // Если файл не загружен — используем изображение по умолчанию
            else city.ImageUrl = "/images/empty.png";
            // Добавляем новый город в базу данных
            await _dbContext.Cities.AddAsync(city);
            // Загружаем страну по выбранному идентификатору, включая список её городов
            var country = await _dbContext.Countries.
                Include(c => c.Cities).
                FirstOrDefaultAsync(c => c.Id == city.CountryId);

            // Если страна найдена — добавляем город в коллекцию Cities этой страны
            country?.Cities.Add(city); // Если country != null, то добавить city в Cities
            // Сохраняем данные в базе данных
            await _dbContext.SaveChangesAsync();
            // Устанавливаем сообщение об успехе
            TempData["success"] = "The city has been created successfully";
            // Перенаправляем пользователя на представление Index контроллера CityController
            return RedirectToAction(nameof(Index), "City");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue) return NotFound();
            City cityFromDb = await _dbContext.Cities.FindAsync(id.Value);
            if (cityFromDb == null) return NotFound();
            // Список стран для выбора
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
            var cityFromDb = await _dbContext.Cities.FindAsync(city.Id);
            if (cityFromDb == null) return NotFound();
            // Обновляем нужные поля
            cityFromDb.Name = city.Name;
            cityFromDb.Description = city.Description;
            cityFromDb.CountryId = city.CountryId;
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null && file.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string cityPath = Path.Combine(wwwRootPath, "images");
                using (var fileStream = new FileStream(Path.Combine(cityPath, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                // При загрузке нового файла можно удалить старый файл  
                if (!string.IsNullOrEmpty(cityFromDb.ImageUrl) && cityFromDb.ImageUrl != "/images/empty.png")
                {
                    string oldImagePath = Path.Combine(wwwRootPath, cityFromDb.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                cityFromDb.ImageUrl = "/images/" + fileName;
            }
            else if (string.IsNullOrEmpty(cityFromDb.ImageUrl)) cityFromDb.ImageUrl = "/images/empty.png";            
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

            if (!string.IsNullOrEmpty(cityToBeDeleted.ImageUrl))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, cityToBeDeleted.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
            }

            _dbContext.Cities.Remove(cityToBeDeleted);
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "The city has been deleted successfully";
            return RedirectToAction(nameof(Index), "City");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int? cityId)
        {
            City city = await _dbContext.Cities.FindAsync(cityId);
            if (city == null) return NotFound();
            string imageUrl = city.ImageUrl;
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
            catch(Exception ex)
            {
                _logger.LogWarning($"Error: {ex.Message}");
            }
            // Обнуляем ссылку на фото в БД
            _dbContext.Cities.Update(city);
            city.ImageUrl = null;
            await _dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK);
        }
    }
}

