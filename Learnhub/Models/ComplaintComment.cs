using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnhub.Models
{
    public class ComplaintComment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ComplaintTopic")]
        public int ComplaintTopicId { get; set; }

        public required ComplaintTopic ComplaintTopic { get; set; }  // Required reference type attribute

        [ForeignKey("User")]
        public int UserId { get; set; }

        public required User User { get; set; }  //Required reference type attribute

        [Required]
        public required string Content { get; set; }  // Required string to prevent null values

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int LikeCount { get; set; } = 0;
    }
}

