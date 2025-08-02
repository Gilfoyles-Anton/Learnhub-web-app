using System;
using System.Collections.Generic;

namespace Learnhub.Models
{
    public class ComplaintTopicDetailViewModel
    {
        // Complaint Subject Field
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;        // Complaint Title
        public string Description { get; set; } = string.Empty;  // Complaint content or description
        public DateTime CreatedAt { get; set; }
        public int Likes { get; set; } = 0;
        public int Views { get; set; } = 0;
        public int ParticipantsCount { get; set; } = 0;
        public string CreatedBy { get; set; } = string.Empty;    // Poster's username or ID
        public string CoverImagePath { get; set; } = string.Empty;

        public bool IsPinned { get; set; }  // Add this attribute

        public List<CommentViewModel> Comments { get; set; } = new();  // Add this attribute to ensure that it is part of the comment list.
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalComments { get; set; } = 0;
    }
}
