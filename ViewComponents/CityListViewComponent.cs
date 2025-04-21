using AsiaGuides.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsiaGuides.ViewComponents
{
    // Компонент отображения списка городов
    public class CityListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _dbContext;
        // Через конструктор внедряется контекст базы данных
        public CityListViewComponent(ApplicationDbContext _dbContext)
        {
            this._dbContext = _dbContext;
        }
        // Метод вызывается из Razor-представления — возвращает список городов. Используем в Layout
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cities = await _dbContext.Cities.ToListAsync();
            return View(cities);
        }
    }
}
