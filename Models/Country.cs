using System.ComponentModel.DataAnnotations;

namespace AsiaGuides.Models
{
    public class Country
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public List<City> Cities { get; set; } = new();
    }
}
