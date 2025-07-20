using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Learnhub.Controllers
{
    public class SearchController : Controller
    {
        public IActionResult Results(string query)
        {
            // Simulated search data from the database (can be replaced with actual database queries)
            var allCourses = new List<string>
            {
                "C# Beginner Tutorial",
                "MVC Framework Explanation",
                "SQL Server Database Optimization",
                "Project Management and Decision Making"
            };

            // Search based on user input keywords
            var results = allCourses
                .Where(course => course.Contains(query, System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            return View(results); // Pass search results to the view
        }
    }
}
