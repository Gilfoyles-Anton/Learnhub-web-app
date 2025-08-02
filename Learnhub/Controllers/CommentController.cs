using Learnhub.Data;
using Learnhub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Learnhub.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Add(int courseId, string content)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(content))
                return RedirectToAction("PlayCourse", "Course", new { id = courseId });

            var userId = int.Parse(userIdStr);

            var comment = new CourseComment
            {
                CourseId = courseId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.CourseComments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("PlayCourse", "Course", new { id = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> Like(int commentId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return Json(new { success = false });

            int userId = int.Parse(userIdStr);

            var exists = await _context.CourseCommentLikes
                .AnyAsync(l => l.CommentId == commentId && l.UserId == userId);

            if (exists)
                return Json(new { success = false, message = "Already liked" });

            _context.CourseCommentLikes.Add(new CourseCommentLike
            {
                CommentId = commentId,
                UserId = userId
            });

            var comment = await _context.CourseComments.FindAsync(commentId);
            if (comment == null)
            {
                return Json(new { success = false, message = "Comment not found" });
            }

            comment.LikeCount += 1;
            await _context.SaveChangesAsync();

            return Json(new { success = true, newLikeCount = comment.LikeCount });
        }

    }
}
