using System.ComponentModel.DataAnnotations;

namespace AsiaGuides.Models
{
    public class AttractionImage
    {
        [Key]
        public int Id { get; set; }
        // Обязательное поле — путь к изображению, которое отображается на сайте (например, "/images/filename.jpg")
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        // Внешний ключ — указывает, к какой достопримечательности относится изображение
        public int AttractionId { get; set; }
        // Навигационное свойство — позволяет получить сам объект Attraction, к которому относится изображение
        public Attraction? Attraction { get; set; }
    }
}
