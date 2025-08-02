using System;
using System.Collections.Generic;

namespace Learnhub.Models
{
    public class ForumTopicDetailViewModel
    {
        // Forum Topic Field
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

        // Comment Page Related
        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalComments { get; set; }
    }
}
