using AsiaGuides.Data;
using AsiaGuides.Utility;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsiaGuides.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Cloudinary _cloudinary;

        public MaintenanceController(ApplicationDbContext _dbContext, IWebHostEnvironment _webHostEnvironment, Cloudinary _cloudinary)
        {
            this._dbContext = _dbContext;
            this._webHostEnvironment = _webHostEnvironment;
            this._cloudinary = _cloudinary;
        }

        [HttpPost]
        public async Task<IActionResult> CleanUnusedImages()
        {
            var usedImageUrls = new HashSet<string>();
            // Страны
            // Получаем список путей к изображениям, используемым в таблице Countries,
            // исключая те записи, у которых ImageUrl пустой или null.
            var countryImages = await _dbContext.Countries
            // Оставляем только те города, у которых ImageUrl указан
            .Where(c => !string.IsNullOrEmpty(c.ImageUrl))
            // Убираем ведущий символ '/' из пути к файлу, чтобы он соответствовал пути в файловой системе
            .Select(c => c.ImageUrl)
            // Выполняем запрос к базе данных и получаем список строк
            .ToListAsync();


            // Города
            // Получаем список путей к изображениям, используемым в таблице Cities,
            // исключая те записи, у которых ImageUrl пустой или null.
            var cityImages = await _dbContext.Cities
            // Оставляем только те города, у которых ImageUrl указан
            .Where(c => !string.IsNullOrEmpty(c.ImageUrl))
            // Убираем ведущий символ '/' из пути к файлу, чтобы он соответствовал пути в файловой системе
            .Select(c => c.ImageUrl)
            // Выполняем запрос к базе данных и получаем список строк
            .ToListAsync();

            // Достопримечательности 

            var attractionImages = await _dbContext.AttractionImage
                .Where(a => !string.IsNullOrEmpty(a.ImageUrl))
                .Select(a => a.ImageUrl)
                .ToListAsync();

            var allCloudinaryResources = new List<Resource>();
            string nextCursor = null;

            do
            {
                var listParams = new ListResourcesParams
                {
                    Type = "upload",
                    MaxResults = 500,
                    NextCursor = nextCursor
                };

                var result = await _cloudinary.ListResourcesAsync(listParams);

                if (result.Resources != null)
                    allCloudinaryResources.AddRange(result.Resources);

                nextCursor = result.NextCursor;

            } while (!string.IsNullOrEmpty(nextCursor));

            //List<string> allImages = new List<string>();
            //allImages.AddRange(countryImages);
            //allImages.AddRange(cityImages);
            //allImages.AddRange(attractionImages);

            //string imagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            //var allFiles = Directory.GetFiles(imagesPath);

            int deletedCount = 0;
            foreach (var resource in allCloudinaryResources)
            {
                string secureUrl = resource.SecureUrl.ToString();

                if (!usedImageUrls.Contains(secureUrl) && !secureUrl.EndsWith("empty.png"))
                {
                    var deleteResult = await _cloudinary.DestroyAsync(new DeletionParams(resource.PublicId));
                    if (deleteResult.Result == "ok" || deleteResult.Result == "deleted")
                        deletedCount++;
                }
            }

            TempData["success"] = $"{deletedCount} unused images deleted.";
            return RedirectToAction(nameof(Index), "Country");
        }
    }
}
