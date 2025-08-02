using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnhub.Models
{
    public class UserComplaintTopicInteraction
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public required User User { get; set; } // add required

        [ForeignKey("ComplaintTopic")]
        public int ComplaintTopicId { get; set; }

        public required ComplaintTopic ComplaintTopic { get; set; } // add required

        public bool IsLiked { get; set; } = true;

        public int LikeCount { get; set; } = 1;

        public bool IsParticipant { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

