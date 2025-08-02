using Learnhub.Data;
using Learnhub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Learnhub.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Like this course
        [HttpPost]
        public async Task<IActionResult> LikeCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.Likes += 1;

            string? sessionUserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrWhiteSpace(sessionUserId) && int.TryParse(sessionUserId, out int userId))
            {
                var interaction = await _context.UserCourseInteractions
                    .FirstOrDefaultAsync(i => i.UserId == userId && i.CourseId == id);

                if (interaction == null)
                {
                    interaction = new UserCourseInteraction
                    {
                        UserId = userId,
                        CourseId = id,
                        IsLiked = true,
                        LikeCount = 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserCourseInteractions.Add(interaction);
                }
                else
                {
                    if (!interaction.IsLiked) interaction.IsLiked = true;
                    interaction.LikeCount += 1;
                    interaction.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { likes = course.Likes });
        }

        // Increase the number of course views
        [HttpPost]
        public async Task<IActionResult> AddView(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.Views += 1;

            string? sessionUserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrWhiteSpace(sessionUserId) && int.TryParse(sessionUserId, out int userId))
            {
                var interaction = await _context.UserCourseInteractions
                    .FirstOrDefaultAsync(i => i.UserId == userId && i.CourseId == id);

                if (interaction == null)
                {
                    interaction = new UserCourseInteraction
                    {
                        UserId = userId,
                        CourseId = id,
                        LastWatchedAt = DateTime.UtcNow,
                        TotalWatchTimeSeconds = 30,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserCourseInteractions.Add(interaction);
                }
                else
                {
                    interaction.LastWatchedAt = DateTime.UtcNow;
                    interaction.TotalWatchTimeSeconds += 30;
                    interaction.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // Save or Unsave the course
        [HttpPost]
        public async Task<IActionResult> ToggleFavoriteCourse(int id)
        {
            string? sessionUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(sessionUserId) || !int.TryParse(sessionUserId, out int userId))
                return Unauthorized();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var interaction = await _context.UserCourseInteractions
                .FirstOrDefaultAsync(i => i.UserId == userId && i.CourseId == id);

            if (interaction == null)
            {
                interaction = new UserCourseInteraction
                {
                    UserId = userId,
                    CourseId = id,
                    IsFavorited = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserCourseInteractions.Add(interaction);
            }
            else
            {
                interaction.IsFavorited = !interaction.IsFavorited;
                interaction.UpdatedAt = DateTime.UtcNow;
            }

            var favoriteCount = await _context.UserCourseInteractions
                .CountAsync(i => i.CourseId == id && i.IsFavorited);

            course.Favorites = favoriteCount;
            await _context.SaveChangesAsync();

            return Json(new
            {
                favorites = favoriteCount,
                isFavorited = interaction.IsFavorited
            });
        }

        // Purchase a course
        [HttpPost]
        public async Task<IActionResult> PurchaseCourse(int id)
        {
            string? sessionUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(sessionUserId) || !int.TryParse(sessionUserId, out int userId))
                return Unauthorized();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var interaction = await _context.UserCourseInteractions
                .FirstOrDefaultAsync(i => i.UserId == userId && i.CourseId == id);

            if (interaction?.IsPurchased == true)
                return Json(new { success = true, alreadyPurchased = true });

            if (user.WalletBalance < course.Price)
                return Json(new { success = false, message = "Insufficient balance. Please recharge." });

            user.WalletBalance -= course.Price;
            course.Purchases += 1;

            if (interaction == null)
            {
                interaction = new UserCourseInteraction
                {
                    UserId = userId,
                    CourseId = id,
                    IsPurchased = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserCourseInteractions.Add(interaction);
            }
            else
            {
                interaction.IsPurchased = true;
                interaction.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, alreadyPurchased = false });
        }

        // Course playback page: Load comments for the current page and retain pagination.
        [HttpGet]
        public async Task<IActionResult> PlayCourse(int id, int pageNumber = 1)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            // Logged-in user
            string? sessionUserId = HttpContext.Session.GetString("UserId");
            int? currentUserId = null;
            if (!string.IsNullOrEmpty(sessionUserId) && int.TryParse(sessionUserId, out int uid))
                currentUserId = uid;

            // Page setting
            int pageSize = 10;
            int skip = (pageNumber - 1) * pageSize;

            // Total number of comments
            var totalComments = await _context.CourseComments.CountAsync(c => c.CourseId == id);

            // Query the comments on the current page
            var commentsRaw = await _context.CourseComments
                .Include(c => c.User)
                .Where(c => c.CourseId == id)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // Which users have made a purchase?
            var userIds = commentsRaw.Select(c => c.UserId).Distinct().ToList();
            var purchasedSet = new HashSet<int>(
                await _context.UserCourseInteractions
                    .Where(i => i.CourseId == id && i.IsPurchased && userIds.Contains(i.UserId))
                    .Select(i => i.UserId)
                    .ToListAsync()
            );

            // Comments that the current user has liked
            var likedCommentIds = currentUserId.HasValue
                ? await _context.CourseCommentLikes
                    .Where(l => l.UserId == currentUserId.Value)
                    .Select(l => l.CommentId).ToListAsync()
                : new List<int>();

            // Project onto the ViewModel
            var comments = commentsRaw.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                LikeCount = c.LikeCount,
                UserName = c.User.Username,
                UserAvatarUrl = string.IsNullOrEmpty(c.User.BackgroundImagePath)
                    ? "/images/default-avatar.png"
                    : (c.User.BackgroundImagePath.StartsWith('/')
                       ? c.User.BackgroundImagePath
                       : "/images/" + c.User.BackgroundImagePath),
                UserHasPurchased = purchasedSet.Contains(c.UserId),
                CurrentUserLiked = likedCommentIds.Contains(c.Id)
            }).ToList();

            var model = new CourseDetailViewModel
            {
                Course = course,
                Comments = comments,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalComments = totalComments
            };

            return View(model);
        }


        // Like and comment
        [HttpPost]
        public async Task<IActionResult> ToggleCommentLike(int id)
        {
            string? sessionUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(sessionUserId) || !int.TryParse(sessionUserId, out int userId))
                return Unauthorized();

            var comment = await _context.CourseComments.FindAsync(id);
            if (comment == null) return NotFound();

            var existing = await _context.CourseCommentLikes
                .FirstOrDefaultAsync(l => l.CommentId == id && l.UserId == userId);

            if (existing == null)
            {
                _context.CourseCommentLikes.Add(new CourseCommentLike
                {
                    CommentId = id,
                    UserId = userId,
                    LikedAt = DateTime.UtcNow
                });
                comment.LikeCount += 1;
            }
            else
            {
                _context.CourseCommentLikes.Remove(existing);
                comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, likeCount = comment.LikeCount });
        }

        // Add a new comment
        [HttpPost]
        public async Task<IActionResult> AddComment(int courseId, string content)
        {
            string? userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(content))
                return BadRequest("User not logged in or empty comment.");

            if (!int.TryParse(userIdStr, out int userId))
                return BadRequest("Invalid user.");

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var comment = new CourseComment
            {
                CourseId = courseId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CourseComments.Add(comment);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
