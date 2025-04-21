using AsiaGuides.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsiaGuides.ViewComponents
{
    public class AttractionListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _dbContext;
        public AttractionListViewComponent(ApplicationDbContext _dbContext)
        {
            this._dbContext = _dbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var attractions = await _dbContext.Attractions.ToListAsync();
            return View(attractions);
        }
    }
}
