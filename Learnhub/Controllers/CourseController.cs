using Learnhub.Data;
using Learnhub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
                    if (!interaction.IsLiked)
                        interaction.IsLiked = true;

                    interaction.LikeCount += 1;
                    interaction.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { likes = course.Likes });
        }

        // Increase the number of views
        [HttpPost]
        public async Task<IActionResult> AddView(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

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

        // Collect or Uncollect
        [HttpPost]
        public async Task<IActionResult> ToggleFavoriteCourse(int id)
        {
            string? sessionUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(sessionUserId) || !int.TryParse(sessionUserId, out int userId))
                return Unauthorized();

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

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

            // Have you made the purchase?
            var interaction = await _context.UserCourseInteractions
                .FirstOrDefaultAsync(i => i.UserId == userId && i.CourseId == id);

            if (interaction?.IsPurchased == true)
            {
                return Json(new { success = true, alreadyPurchased = true });
            }

            if (user.WalletBalance < course.Price)
            {
                return Json(new { success = false, message = "Insufficient balance in the wallet. Please recharge." });
            }

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







    }
}

