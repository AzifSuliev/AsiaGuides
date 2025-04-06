using AsiaGuides.Data;
using AsiaGuides.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsiaGuides.ViewComponents
{
    public class CountryListViewComponent: ViewComponent
    {
        private readonly ApplicationDbContext _dbContext;

        public CountryListViewComponent(ApplicationDbContext _dbContext)
        {
            this._dbContext = _dbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var countries = await _dbContext.Countries.ToListAsync();
            return View(countries);
        }
    }
}
