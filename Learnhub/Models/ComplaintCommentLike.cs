using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnhub.Models
{
    public class ComplaintCommentLike
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ComplaintComment")]
        public int CommentId { get; set; }

        public required ComplaintComment ComplaintComment { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public required User User { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.Now;
    }
}

