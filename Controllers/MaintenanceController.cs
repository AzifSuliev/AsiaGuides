using AsiaGuides.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsiaGuides.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public MaintenanceController(ApplicationDbContext _dbContext, IWebHostEnvironment _webHostEnvironment)
        {
            this._dbContext = _dbContext;
            this._webHostEnvironment = _webHostEnvironment;
        }

        [HttpPost]
        public async Task<IActionResult> CleanUnusedImages()
        {
            // Страны
            // Получаем список путей к изображениям, используемым в таблице Countries,
            // исключая те записи, у которых ImageUrl пустой или null.
            var countryImages = await _dbContext.Countries
            // Оставляем только те города, у которых ImageUrl указан
            .Where(c => !string.IsNullOrEmpty(c.ImageUrl))
            // Убираем ведущий символ '/' из пути к файлу, чтобы он соответствовал пути в файловой системе
            .Select(c => c.ImageUrl.TrimStart('/'))
            // Выполняем запрос к базе данных и получаем список строк
            .ToListAsync();


            // Города
            // Получаем список путей к изображениям, используемым в таблице Cities,
            // исключая те записи, у которых ImageUrl пустой или null.
            var cityImages = await _dbContext.Cities
            // Оставляем только те города, у которых ImageUrl указан
            .Where(c => !string.IsNullOrEmpty(c.ImageUrl))
            // Убираем ведущий символ '/' из пути к файлу, чтобы он соответствовал пути в файловой системе
            .Select(c => c.ImageUrl.TrimStart('/'))
            // Выполняем запрос к базе данных и получаем список строк
            .ToListAsync();

            // Достопримечательности 

            var attractionImages = await _dbContext.AttractionImage
                .Where(a => !string.IsNullOrEmpty(a.ImageUrl))
                .Select(a => a.ImageUrl.TrimStart('/'))
                .ToListAsync();

            List<string> allImages = new List<string>();
            allImages.AddRange(countryImages);
            allImages.AddRange(cityImages);
            allImages.AddRange(attractionImages);

            string imagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            var allFiles = Directory.GetFiles(imagesPath);

            int deletedCount = 0;
            foreach (var file in allFiles)
            {
                string relativePath = "images/" + Path.GetFileName(file);
                if (!allImages.Contains(relativePath) && Path.GetFileName(file) != "empty.png")
                {
                    System.IO.File.Delete(file);
                    deletedCount++;
                }
            }

            TempData["success"] = $"{deletedCount} unused images deleted.";
            return RedirectToAction(nameof(Index), "Country");
        }
    }
}
