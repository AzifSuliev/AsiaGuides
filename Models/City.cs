using System.ComponentModel.DataAnnotations;

namespace AsiaGuides.Models
{
    public class City
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
        public string? PublicId { get; set; }
        [Required(ErrorMessage = "You must select a country")]
        public int CountryId { get; set; }
        public Country? Country { get; set; }
        public List<Attraction> Attractions { get; set; } = new();
    }
}
