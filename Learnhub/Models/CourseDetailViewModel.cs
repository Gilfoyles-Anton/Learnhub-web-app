using System;
using System.Collections.Generic;

namespace Learnhub.Models
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public string UserName { get; set; } = "";
        public string UserAvatarUrl { get; set; } = "/images/default-avatar.png";
        public bool UserHasPurchased { get; set; }
        public bool CurrentUserLiked { get; set; }
    }

    public class CourseDetailViewModel
    {
        public Course Course { get; set; } = new Course();

        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();

        // Search keywords, sorting, pagination and other extensible fields
        public string? SearchKeyword { get; set; }
        public string? SortOrder { get; set; } // "time", "likes" 
        public bool ShowPurchasedOnly { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalComments { get; set; }
    }
}
