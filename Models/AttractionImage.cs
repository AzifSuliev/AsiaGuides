using System.ComponentModel.DataAnnotations;

namespace AsiaGuides.Models
{
    public class AttractionImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public int AttractionId { get; set; }
        public Attraction Attraction { get; set; }
    }
}
