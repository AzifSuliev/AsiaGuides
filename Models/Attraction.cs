using System.ComponentModel.DataAnnotations;

namespace AsiaGuides.Models
{
    public class Attraction
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;
        public int CityId { get; set; }  // Внешний ключ для связи с City
        public City? City { get; set; }  // Связь с моделью City
        public List<AttractionImage> Images { get; set; } = new(); // коллекция фото достопримечательности 
        public double Rating { get; set; }
        public string? OpeningHours { get; set; }
    }
}
