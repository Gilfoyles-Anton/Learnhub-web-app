namespace Learnhub.Models
{
    public class ForumCommentLike
    {
        public int Id { get; set; }
        public int CommentId { get; set; }

        public required ForumComment Comment { get; set; } // add required

        public int UserId { get; set; }

        public required User User { get; set; } // add required

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}

