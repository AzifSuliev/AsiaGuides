using AsiaGuides.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsiaGuides.ViewComponents
{
    public class CityListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _dbContext;

        public CityListViewComponent(ApplicationDbContext _dbContext)
        {
            this._dbContext = _dbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cities = await _dbContext.Cities.ToListAsync();
            return View(cities);
        }
    }
}
