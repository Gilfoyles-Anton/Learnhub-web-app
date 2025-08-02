using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnhub.Models
{
    public class UserForumTopicInteraction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ForumTopicId { get; set; }

        [Required]
        public bool IsLiked { get; set; } = true;

        [Required]
        public int LikeCount { get; set; } = 1;

        public bool IsParticipant { get; set; } = false; // New field added

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Optional: Navigation Properties (based on your project structure)
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("ForumTopicId")]
        public virtual ForumTopic? ForumTopic { get; set; }
    }
}
