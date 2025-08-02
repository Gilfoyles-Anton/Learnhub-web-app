using Learnhub.Data;
using Learnhub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Learnhub.Controllers
{
    public class ComplaintDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComplaintDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ComplaintDetails/{id}
        public IActionResult Index(int id)
        {
            var complaint = _context.ComplaintTopics.FirstOrDefault(c => c.Id == id);
            if (complaint == null) return NotFound();

            complaint.Views++;
            _context.SaveChanges();

            var model = new ComplaintTopicDetailViewModel
            {
                Id = complaint.Id,
                Title = complaint.Title ?? "",
                Description = complaint.Description ?? "",
                CreatedAt = complaint.CreatedAt,
                Likes = complaint.Likes,
                Views = complaint.Views,
                ParticipantsCount = complaint.ParticipantsCount,
                CreatedBy = complaint.CreatedBy ?? "",
                CoverImagePath = complaint.CoverImagePath ?? "",
                IsPinned = false,
                Comments = _context.ComplaintComments
                    .Include(c => c.User)
                    .Where(c => c.ComplaintTopicId == id)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CommentViewModel
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
                        UserHasPurchased = false
                    }).ToList(),
                PageNumber = 1,
                PageSize = 10,
                TotalComments = _context.ComplaintComments.Count(c => c.ComplaintTopicId == id)
            };

            return View("~/Views/Account/ComplaintDetails.cshtml", model);
        }

        // POST: /ComplaintDetails/LikeTopic/5
        [HttpPost]
        public async Task<IActionResult> LikeTopic(int id)
        {
            var topic = await _context.ComplaintTopics.FindAsync(id);
            if (topic == null) return NotFound();

            topic.Likes += 1;
            await _context.SaveChangesAsync();

            return Json(new { likes = topic.Likes });
        }

        // POST: /ComplaintDetails/ParticipateTopic/5
        [HttpPost]
        public async Task<IActionResult> ParticipateTopic(int id)
        {
            var topic = await _context.ComplaintTopics.FindAsync(id);
            if (topic == null) return NotFound();

            string? sessionUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(sessionUserId) || !int.TryParse(sessionUserId, out int userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var interaction = await _context.UserComplaintTopicInteractions
                .FirstOrDefaultAsync(i => i.UserId == userId && i.ComplaintTopicId == id);

            if (interaction == null)
            {
                interaction = new UserComplaintTopicInteraction
                {
                    UserId = userId,
                    User = user,
                    ComplaintTopicId = id,
                    ComplaintTopic = topic,
                    IsParticipant = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserComplaintTopicInteractions.Add(interaction);
                topic.ParticipantsCount += 1;
            }
            else if (!interaction.IsParticipant)
            {
                interaction.IsParticipant = true;
                interaction.UpdatedAt = DateTime.UtcNow;
                topic.ParticipantsCount += 1;
            }

            await _context.SaveChangesAsync();
            return Json(new { participants = topic.ParticipantsCount });
        }

        // POST: /ComplaintDetails/AddComment
        [HttpPost]
        public async Task<IActionResult> AddComment(int complaintTopicId, string content)
        {
            string? sessionUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(sessionUserId) || !int.TryParse(sessionUserId, out int userId))
            {
                return Unauthorized();
            }

            var topic = await _context.ComplaintTopics.FindAsync(complaintTopicId);
            if (topic == null) return NotFound();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var comment = new ComplaintComment
            {
                ComplaintTopicId = complaintTopicId,
                ComplaintTopic = topic,
                Content = content,
                UserId = userId,
                User = user,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ComplaintComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: /ComplaintDetails/ToggleCommentLike/5
        [HttpPost]
        public async Task<IActionResult> ToggleCommentLike(int id)
        {
            string? sessionUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(sessionUserId) || !int.TryParse(sessionUserId, out int userId))
            {
                return Unauthorized();
            }

            var comment = await _context.ComplaintComments
                .Include(c => c.ComplaintTopic)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var existing = await _context.ComplaintCommentLikes
                .FirstOrDefaultAsync(l => l.CommentId == id && l.UserId == userId);

            if (existing == null)
            {
                var like = new ComplaintCommentLike
                {
                    CommentId = id,
                    ComplaintComment = comment,
                    UserId = userId,
                    User = user,
                    LikedAt = DateTime.UtcNow
                };
                _context.ComplaintCommentLikes.Add(like);
                comment.LikeCount += 1;
            }
            else
            {
                _context.ComplaintCommentLikes.Remove(existing);
                comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
            }

            await _context.SaveChangesAsync();
            return Json(new { likeCount = comment.LikeCount });
        }
    }
}
