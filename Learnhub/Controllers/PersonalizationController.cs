using Learnhub.Data;
using Learnhub.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Learnhub.Controllers
{
    public class PersonalizationController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public PersonalizationController(IWebHostEnvironment env, ApplicationDbContext context)
        {
            _env = env;
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var presetDir = Path.Combine(_env.WebRootPath, "images", "backgrounds");
            if (!Directory.Exists(presetDir))
                Directory.CreateDirectory(presetDir);

            var presetImages = Directory.GetFiles(presetDir)
                .Where(f => f.EndsWith(".jpg") || f.EndsWith(".png") || f.EndsWith(".jpeg") || f.EndsWith(".webp"))
                .Select(f => "/images/backgrounds/" + Path.GetFileName(f))
                .ToList();

            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
            ViewBag.CurrentBackground = user?.BackgroundImagePath;

            //  Specify the complete view path
            return View("~/Views/Account/Personalization.cshtml", presetImages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetBackground(string imagePath)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            if (!string.IsNullOrEmpty(imagePath) &&
                (imagePath.StartsWith("/images/backgrounds/") || imagePath.StartsWith("/uploads/backgrounds/")))
            {
                var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
                if (user != null)
                {
                    user.BackgroundImagePath = imagePath;
                    _context.SaveChanges();

                    //  Synchronize and save to Session
                    HttpContext.Session.SetString("BackgroundImagePath", imagePath);
                }
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadBackground(IFormFile customImage)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            if (customImage == null || customImage.Length == 0)
            {
                TempData["UploadError"] = "Please select an image to upload.";
                return RedirectToAction("Index");
            }

            var ext = Path.GetExtension(customImage.FileName).ToLower();
            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!allowedExt.Contains(ext))
            {
                TempData["UploadError"] = "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed.";
                return RedirectToAction("Index");
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "backgrounds");
            Directory.CreateDirectory(uploadsDir);

            var safeFileName = $"{userId}_{Guid.NewGuid()}{ext}";
            var savePath = Path.Combine(uploadsDir, safeFileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await customImage.CopyToAsync(stream);
            }

            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
            if (user != null)
            {
                user.BackgroundImagePath = "/uploads/backgrounds/" + safeFileName;
                _context.SaveChanges();

                //  Synchronize and save to Session
                HttpContext.Session.SetString("BackgroundImagePath", user.BackgroundImagePath);
            }

            return RedirectToAction("Index");
        }

    }
}
