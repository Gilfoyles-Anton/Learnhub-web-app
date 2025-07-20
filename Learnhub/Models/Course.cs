using System.ComponentModel.DataAnnotations;

namespace Learnhub.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0, 10000)]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // New statistics field.Default value is 0
        public int Likes { get; set; } = 0;
        public int Favorites { get; set; } = 0;
        public int Views { get; set; } = 0;
        public int Purchases { get; set; } = 0;

        public int VideoCount { get; set; } = 0;

        public string? VideoFileNames { get; set; }

        public string? VideoFilePaths { get; set; }

        public string? CoverImagePath { get; set; }

    }
}
