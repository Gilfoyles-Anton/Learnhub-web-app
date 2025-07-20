using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnhub.Models
{
    public class UserCourseInteraction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public bool IsPurchased { get; set; } = false;
        public bool IsFavorited { get; set; } = false;
        public bool IsLiked { get; set; } = false;

        public DateTime? LastWatchedAt { get; set; }

        public int TotalWatchTimeSeconds { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int LikeCount { get; set; } = 0;

        // The navigation attribute is marked as optional, eliminating the CS8618 warning.
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
    }
}

