namespace Learnhub.Models
{
    public class ForumComment
    {
        public int Id { get; set; }

        public int ForumTopicId { get; set; }

        public required ForumTopic ForumTopic { get; set; } // Navigation attribute with "required"

        public int UserId { get; set; }

        public required User User { get; set; } // Navigation attribute with "required"

        public required string Content { get; set; } // The content cannot be empty. 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int LikeCount { get; set; } = 0;
    }
}
