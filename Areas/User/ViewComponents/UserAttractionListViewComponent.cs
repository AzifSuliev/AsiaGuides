using AsiaGuides.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsiaGuides.Areas.User.ViewComponents
{
    public class UserAttractionListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _dbContext;
        public UserAttractionListViewComponent(ApplicationDbContext _dbContext)
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
