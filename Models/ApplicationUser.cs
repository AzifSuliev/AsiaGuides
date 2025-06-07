using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AsiaGuides.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? LastName { get; set; }
    }
}
