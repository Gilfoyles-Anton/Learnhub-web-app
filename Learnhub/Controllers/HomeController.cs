using System.Diagnostics;
using Learnhub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Learnhub.Data;

namespace Learnhub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // The home page displays the course list, supporting search, sorting, paging, and view mode switching
        public IActionResult Index(
            string search,
            decimal? minPrice,
            decimal? maxPrice,
            string sortBy,
            string sortOrder,
            int page = 1,
            string viewMode = "table")
        {
            int pageSize = 20;

            var courses = _context.Courses.AsQueryable();

            // Search title or description
            if (!string.IsNullOrEmpty(search))
            {
                courses = courses.Where(c =>
                    (c.Title ?? "").Contains(search) ||
                    (c.Description ?? "").Contains(search));
            }

            // Price range
            if (minPrice.HasValue)
                courses = courses.Where(c => c.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                courses = courses.Where(c => c.Price <= maxPrice.Value);

            // Sorting
            bool isDescending = sortOrder == "desc";

            courses = sortBy switch
            {
                "Title" => isDescending ? courses.OrderByDescending(c => c.Title) : courses.OrderBy(c => c.Title),
                "Description" => isDescending ? courses.OrderByDescending(c => c.Description) : courses.OrderBy(c => c.Description),
                "Price" => isDescending ? courses.OrderByDescending(c => c.Price) : courses.OrderBy(c => c.Price),
                "CreatedAt" => isDescending ? courses.OrderByDescending(c => c.CreatedAt) : courses.OrderBy(c => c.CreatedAt),
                "Likes" => isDescending ? courses.OrderByDescending(c => c.Likes) : courses.OrderBy(c => c.Likes),
                "Favorites" => isDescending ? courses.OrderByDescending(c => c.Favorites) : courses.OrderBy(c => c.Favorites),
                "Views" => isDescending ? courses.OrderByDescending(c => c.Views) : courses.OrderBy(c => c.Views),
                "Purchases" => isDescending ? courses.OrderByDescending(c => c.Purchases) : courses.OrderBy(c => c.Purchases),
                "VideoCount" => isDescending ? courses.OrderByDescending(c => c.VideoCount) : courses.OrderBy(c => c.VideoCount),
                _ => courses.OrderByDescending(c => c.CreatedAt) //By default, it is in descending order by creation time
            };

            // Paging calculation
            int totalItems = courses.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages > 0 ? totalPages : 1;

            var pagedCourses = courses
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // iewBag is used for page paging and state retention
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentSearch = search ?? "";
            ViewBag.CurrentMinPrice = minPrice?.ToString() ?? "";
            ViewBag.CurrentMaxPrice = maxPrice?.ToString() ?? "";
            ViewBag.CurrentSortBy = sortBy ?? "";
            ViewBag.CurrentSortOrder = sortOrder ?? "";
            ViewBag.ViewMode = viewMode ?? "table";

            return View(pagedCourses);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
