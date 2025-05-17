using AsiaGuides.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsiaGuides.Areas.User.ViewComponents
{
    // Компонент отображения списка городов
    public class UserCityListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _dbContext;
        // Через конструктор внедряется контекст базы данных
        public UserCityListViewComponent(ApplicationDbContext _dbContext)
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
