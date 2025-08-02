// Models/ComplaintTopic.cs
namespace Learnhub.Models
{
    public class ComplaintTopic
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Likes { get; set; }
        public int Views { get; set; }
        public int ParticipantsCount { get; set; }
        public string? CreatedBy { get; set; }
        public bool IsPinned { get; set; }
        public string? CoverImagePath { get; set; }
    }
}

