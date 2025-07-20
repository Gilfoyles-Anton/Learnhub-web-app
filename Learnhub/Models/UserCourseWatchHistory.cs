using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnhub.Models
{
    public class UserCourseWatchHistory
    {
        [Key]
        public int Id { get; set; }

        public int InteractionId { get; set; }

        public int VideoIndex { get; set; }

        public DateTime WatchedAt { get; set; }

        public int DurationSeconds { get; set; } = 0;

        public bool WatchedToEnd { get; set; } = false;

        [ForeignKey("InteractionId")]
        public UserCourseInteraction? Interaction { get; set; }
    }
}
