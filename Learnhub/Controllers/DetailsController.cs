using Learnhub.Data;
using Learnhub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learnhub.Controllers
{
    public class DetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Details/{id}?pageNumber=1
        public async Task<IActionResult> Index(int id, int pageNumber = 1)
        {
            var topic = await _context.ForumTopics.FirstOrDefaultAsync(f => f.Id == id);
            if (topic == null)
            {
                return NotFound();
            }

            topic.Views++;
            await _context.SaveChangesAsync();

            int pageSize = 10;
            int skip = (pageNumber - 1) * pageSize;

            var commentsRaw = await _context.ForumComments
                .Include(c => c.User)
                .Where(c => c.ForumTopicId == id)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            string? sessionUserId = HttpContext.Session.GetString("UserId");
            int? currentUserId = null;
            if (!string.IsNullOrEmpty(sessionUserId) && int.TryParse(sessionUserId, out int uid))
                currentUserId = uid;

            var likedCommentIds = currentUserId.HasValue
                ? await _context.ForumCommentLikes
                    .Where(l => l.UserId == currentUserId.Value && commentsRaw.Select(c => c.Id).Contains(l.CommentId))
                    .Select(l => l.CommentId)
                    .ToListAsync()
                : new List<int>();

            var comments = commentsRaw.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                LikeCount = c.LikeCount,
                UserName = c.User.Username,
                UserAvatarUrl = string.IsNullOrEmpty(c.User.BackgroundImagePath)
                    ? "/images/default-avatar.png"
                    : (c.User.BackgroundImagePath.StartsWith("/")
                        ? c.User.BackgroundImagePath
                        : "/images/" + c.User.BackgroundImagePath),
                UserHasPurchased = false,
                CurrentUserLiked = likedCommentIds.Contains(c.Id)
            }).ToList();

            var totalComments = await _context.ForumComments.CountAsync(c => c.ForumTopicId == id);

            var model = new ForumTopicDetailViewModel
            {
                Id = topic.Id,
                Title = topic.Title,
                Description = topic.Description,
                CreatedAt = topic.CreatedAt,
                Likes = topic.Likes,
                Views = topic.Views,
                ParticipantsCount = topic.ParticipantsCount,
                CreatedBy = topic.CreatedBy,
                IsPinned = topic.IsPinned,
                CoverImagePath = topic.CoverImagePath,
                Comments = comments,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalComments = totalComments
            };

            return View("~/Views/Account/Details.cshtml", model);
        }


        // Add a comment
        [HttpPost]
        public async Task<IActionResult> AddComment(int forumTopicId, string content)
        {
            string? userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(content))
                return BadRequest("用户未登录或评论为空");

            if (!int.TryParse(userIdStr, out int userId))
                return BadRequest("用户ID无效");

            var topic = await _context.ForumTopics.FindAsync(forumTopicId);
            if (topic == null) return NotFound();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return BadRequest("用户不存在");

            var comment = new ForumComment
            {
                ForumTopicId = forumTopicId,
                UserId = userId,
                ForumTopic = topic,  // It is necessary to assign a value.
                User = user,         // It is necessary to assign a value.
                Content = content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ForumComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok();
        }




        [HttpPost]
        public async Task<IActionResult> LikeTopic(int id)
        {
            var topic = await _context.ForumTopics.FindAsync(id);
            if (topic == null) return NotFound();

            topic.Likes += 1;

            string? sessionUserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrWhiteSpace(sessionUserId) && int.TryParse(sessionUserId, out int userId))
            {
                var existing = await _context.UserForumTopicInteractions
                    .FirstOrDefaultAsync(i => i.UserId == userId && i.ForumTopicId == id);

                if (existing == null)
                {
                    existing = new UserForumTopicInteraction
                    {
                        UserId = userId,
                        ForumTopicId = id,
                        IsLiked = true,
                        LikeCount = 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserForumTopicInteractions.Add(existing);
                }
                else
                {
                    if (!existing.IsLiked) existing.IsLiked = true;
                    existing.LikeCount += 1;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { likes = topic.Likes });
        }


        [HttpPost]
        public async Task<IActionResult> ParticipateTopic(int id)
        {
            var topic = await _context.ForumTopics.FindAsync(id);
            if (topic == null) return NotFound();

            string? sessionUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(sessionUserId) || !int.TryParse(sessionUserId, out int userId))
            {
                return Unauthorized();
            }

            var interaction = await _context.UserForumTopicInteractions
                .FirstOrDefaultAsync(i => i.UserId == userId && i.ForumTopicId == id);

            if (interaction == null)
            {
                interaction = new UserForumTopicInteraction
                {
                    UserId = userId,
                    ForumTopicId = id,
                    IsLiked = false,
                    LikeCount = 0,
                    IsParticipant = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserForumTopicInteractions.Add(interaction);
                topic.ParticipantsCount += 1;
            }
            else if (!interaction.IsParticipant)
            {
                interaction.IsParticipant = true;
                interaction.UpdatedAt = DateTime.UtcNow;
                topic.ParticipantsCount += 1;
            }
            else
            {
                // Already a participant, no further actions or prompts are required.
                return Json(new { participants = topic.ParticipantsCount });
            }

            await _context.SaveChangesAsync();
            return Json(new { participants = topic.ParticipantsCount });
        }








        // Like/Dislike
        [HttpPost]
        public async Task<IActionResult> ToggleCommentLike(int id)
        {
            string? sessionUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(sessionUserId) || !int.TryParse(sessionUserId, out int userId))
                return Unauthorized();

            var comment = await _context.ForumComments.FindAsync(id);
            if (comment == null) return NotFound();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var existing = await _context.ForumCommentLikes
                .FirstOrDefaultAsync(l => l.CommentId == id && l.UserId == userId);

            if (existing == null)
            {
                _context.ForumCommentLikes.Add(new ForumCommentLike
                {
                    CommentId = id,
                    UserId = userId,
                    Comment = comment,  //It is necessary to assign a value.
                    User = user,        // It is necessary to assign a value.
                    LikedAt = DateTime.UtcNow
                });
                comment.LikeCount += 1;
            }
            else
            {
                _context.ForumCommentLikes.Remove(existing);
                comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, likeCount = comment.LikeCount });
        }

    }
}
