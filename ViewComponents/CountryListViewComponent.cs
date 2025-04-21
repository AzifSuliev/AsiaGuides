using AsiaGuides.Data;
using AsiaGuides.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsiaGuides.ViewComponents
{
    // Компонент отображения списка стран
    public class CountryListViewComponent: ViewComponent
    {
        private readonly ApplicationDbContext _dbContext;
        // Через конструктор внедряется контекст базы данных
        public CountryListViewComponent(ApplicationDbContext _dbContext)
        {
            this._dbContext = _dbContext;
        }
        // Метод вызывается из Razor-представления — возвращает список стран. Используем в Layout
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var countries = await _dbContext.Countries.ToListAsync();
            return View(countries);
        }
    }
}
