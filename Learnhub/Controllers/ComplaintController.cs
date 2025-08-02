using Learnhub.Data;
using Learnhub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System;

namespace Learnhub.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComplaintController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, string sortBy, string sortOrder, int page = 1, string viewMode = "table")
        {
            int pageSize = 20;
            var topics = _context.ComplaintTopics.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                topics = topics.Where(t =>
                    (t.Title ?? "").Contains(search) ||
                    (t.Description ?? "").Contains(search));
            }

            bool isDesc = sortOrder == "desc";
            topics = sortBy switch
            {
                "Title" => isDesc ? topics.OrderByDescending(t => t.Title) : topics.OrderBy(t => t.Title),
                "CreatedAt" => isDesc ? topics.OrderByDescending(t => t.CreatedAt) : topics.OrderBy(t => t.CreatedAt),
                "Likes" => isDesc ? topics.OrderByDescending(t => t.Likes) : topics.OrderBy(t => t.Likes),
                "Views" => isDesc ? topics.OrderByDescending(t => t.Views) : topics.OrderBy(t => t.Views),
                "ParticipantsCount" => isDesc ? topics.OrderByDescending(t => t.ParticipantsCount) : topics.OrderBy(t => t.ParticipantsCount),
                _ => topics.OrderByDescending(t => t.CreatedAt)
            };

            int totalItems = topics.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));
            var pagedTopics = topics.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentSearch = search ?? "";
            ViewBag.CurrentSortBy = sortBy ?? "";
            ViewBag.CurrentSortOrder = sortOrder ?? "";
            ViewBag.ViewMode = viewMode ?? "table";

            return View("~/Views/Account/ComplaintTopic.cshtml", pagedTopics);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return View("~/Views/Account/EditComplaintTopic.cshtml", new ComplaintTopic { CreatedAt = DateTime.Now });

            var topic = await _context.ComplaintTopics.FindAsync(id);
            if (topic == null)
                return NotFound();

            return View("~/Views/Account/EditComplaintTopic.cshtml", topic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ComplaintTopic model, IFormFile CoverImage)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Account/EditComplaintTopic.cshtml", model);

            string coverImagePath = model.CoverImagePath ?? string.Empty;

            if (CoverImage != null && CoverImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "complaint");
                Directory.CreateDirectory(uploadsFolder);
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(CoverImage.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await CoverImage.CopyToAsync(stream);
                }

                coverImagePath = "/uploads/complaint/" + fileName;
            }

            if (model.Id == 0)
            {
                model.CreatedAt = DateTime.Now;
                model.CoverImagePath = coverImagePath;
                _context.ComplaintTopics.Add(model);
            }
            else
            {
                var existing = await _context.ComplaintTopics.FindAsync(model.Id);
                if (existing == null) return NotFound();

                existing.Title = model.Title;
                existing.Description = model.Description;
                existing.IsPinned = model.IsPinned;
                existing.CreatedBy = model.CreatedBy;
                if (!string.IsNullOrEmpty(coverImagePath)) existing.CoverImagePath = coverImagePath;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Complaint saved successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var topic = await _context.ComplaintTopics.FindAsync(id);
            if (topic != null)
            {
                _context.ComplaintTopics.Remove(topic);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
