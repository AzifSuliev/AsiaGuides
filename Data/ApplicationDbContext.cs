using AsiaGuides.Models;
using Microsoft.EntityFrameworkCore;

namespace AsiaGuides.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Attraction> Attractions { get; set; }
        public DbSet<AttractionImage> AttractionImage { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().HasData(
                new Country()
                {
                    Id = 1,
                    Name = "Kazakhstan",
                    Description = "Kazakhstan is the largest country in Central Asia."            
                },
                new Country()
                {
                    Id = 2,
                    Name = "Uzbekistan",
                    Description = "Uzbekistan is a country with a rich historical heritage."
                }
            );
        }
    }
}
