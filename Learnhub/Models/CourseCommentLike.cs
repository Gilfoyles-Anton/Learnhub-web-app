using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnhub.Models
{
    public class CourseCommentLike
    {
        public int Id { get; set; }

        public int CommentId { get; set; }
        public int UserId { get; set; }
        public DateTime LikedAt { get; set; } = DateTime.Now;

        [ForeignKey("CommentId")]
        public CourseComment Comment { get; set; } = null!;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}

