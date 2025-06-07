using System.ComponentModel.DataAnnotations;

namespace AsiaGuides.Models
{
    public class City
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        public string? Name { get; set; }
        [Required]
        [MaxLength(100)]
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        [Required(ErrorMessage = "You must select a country")]
        public int CountryId { get; set; }
        public Country? Country { get; set; }
        public List<Attraction> Attractions { get; set; } = new();
    }
}
