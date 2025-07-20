using Learnhub.Data;
using Learnhub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Learnhub.Controllers;

public class AccountController(ApplicationDbContext _context) : Controller
{
    // Registration
    [HttpGet]
    public IActionResult Register()
    {
        return View(new User());
    }

    [HttpPost]
    public async Task<IActionResult> Register(User model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("", "Email already exists.");
            return View(model);
        }

        model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
        _context.Users.Add(model);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(model.AdminActivationCode))
        {
            var activationCode = _context.AdminActivationCodes
                .FirstOrDefault(a => a.ActivationCode == model.AdminActivationCode && a.Status == "unused");

            if (activationCode == null)
            {
                ModelState.AddModelError("", "Invalid or already used activation code.");
                return View(model);
            }

            model.Role = "Admin";
            activationCode.Status = "used";
            activationCode.UsedByUserId = model.Id;
            _context.AdminActivationCodes.Update(activationCode);
            await _context.SaveChangesAsync();
        }
        else
        {
            model.Role = "User";
        }

        TempData["SuccessMessage"] = "Registration successful, please log in!";
        return RedirectToAction("Login");
    }

    // Login
    [HttpGet]
    public IActionResult Login()
    {
        return View(new User());
    }

    [HttpPost]
    public IActionResult Login(User model)
    {
        if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.PasswordHash))
        {
            ModelState.AddModelError("", "Email or password cannot be empty.");
            return View(model);
        }

        // Search for users
        var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
        if (user == null)
        {
            ModelState.AddModelError("Email", "Account does not exist.");
            return View(model);
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(model.PasswordHash, user.PasswordHash))
        {
            ModelState.AddModelError("PasswordHash", "Incorrect password.");
            return View(model);
        }

        // Login successful. Setting Session.
        HttpContext.Session.SetString("UserId", user.Id.ToString());

        // Add: Set the path of the background image to Session (if it has been set)
        if (!string.IsNullOrEmpty(user.BackgroundImagePath))
        {
            HttpContext.Session.SetString("BackgroundImagePath", user.BackgroundImagePath);
        }
        else
        {
            // If the user has not set a personalized background image, a default value can be set.
            HttpContext.Session.SetString("BackgroundImagePath", "/images/backgrounds/default.jpg");
        }

        return RedirectToAction("Index", "Home");
    }


    // Logout
    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    // Profile View
    [HttpGet]
    public IActionResult Profile()
    {
        //Get the Session ID of the currently logged-in user.
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login");

        // Search for the corresponding user in the database.
        var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
        if (user == null)
            return RedirectToAction("Login");

        // Load all the course interaction data of this user (likes, collections, purchases, and viewing history).
        var interactions = _context.UserCourseInteractions
            .Include(i => i.Course)
            .Where(i => i.UserId == user.Id)  // Note: Here, we are using user.Id (which is usually a Guid or an int)
            .ToList();

        // Group the interactive information by category and pass it to the view.
        ViewBag.FavoriteCourses = interactions
            .Where(i => i.IsFavorited)
            .Select(i => i.Course)
            .ToList();

        ViewBag.LikedCourses = interactions
            .Where(i => i.IsLiked)
            .Select(i => i.Course)
            .ToList();

        ViewBag.PurchasedCourses = interactions
            .Where(i => i.IsPurchased)
            .Select(i => i.Course)
            .ToList();

        ViewBag.WatchedHistory = interactions
            .Where(i => i.TotalWatchTimeSeconds > 0 || i.LastWatchedAt != null)
            .Select(i => new { Interaction = i, Course = i.Course })
            .ToList();

        ViewBag.UserId = user.Id; // Can be used in the view to identify the current user

        // Return to different pages based on the role.
        if (user.Role == "Admin")
            return View("AdminProfile", user);
        else
            return View("UserProfile", user);
    }


    // Settings View
    [HttpGet]
    public IActionResult Settings()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

        var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
        if (user == null) return RedirectToAction("Login");

        return View(user.Role == "Admin" ? "AdminSettings" : "UserSettings", new UserSettingsViewModel());
    }

    // Settings Submission
    [HttpPost]
    public async Task<IActionResult> Settings(UserSettingsViewModel model)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

        var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
        if (user == null) return RedirectToAction("Login");

        // Verify email and phone number
        if (user.Email != model.OldEmail || user.PhoneNumber != model.OldPhone)
        {
            ModelState.AddModelError("", "Email or phone verification failed");
            return View(user.Role == "Admin" ? "AdminSettings" : "UserSettings", model);
        }

        // Verify old password
        if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
        {
            ModelState.AddModelError("", "Incorrect current password");
            return View(user.Role == "Admin" ? "AdminSettings" : "UserSettings", model);
        }

        // Update fields
        if (!string.IsNullOrEmpty(model.NewUsername)) user.Username = model.NewUsername;
        if (!string.IsNullOrEmpty(model.NewEmail)) user.Email = model.NewEmail;
        if (!string.IsNullOrEmpty(model.NewPhone)) user.PhoneNumber = model.NewPhone;
        if (!string.IsNullOrEmpty(model.NewPassword)) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

        // Upload the avatar to /wwwroot/images/
        if (model.NewAvatar != null)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{user.Id}_avatar.png");
            await using var stream = new FileStream(path, FileMode.Create);
            await model.NewAvatar.CopyToAsync(stream);
        }

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Settings updated successfully";
        return RedirectToAction("Profile");
    }


    // Avatar Upload (standalone)
    [HttpPost]
    public async Task<IActionResult> UpdateAvatar(IFormFile? NewAvatar)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || NewAvatar == null || NewAvatar.Length == 0)
        {
            TempData["ErrorMessage"] = "Avatar upload failed: Please log in and select a file.";
            return RedirectToAction("Settings");
        }

        var user = await _context.Users.FindAsync(int.Parse(userId));
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Settings");
        }

        // Build the avatar path
        var avatarFileName = $"{user.Id}_avatar.png";
        var avatarFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", avatarFileName);

        // Store avatar file
        await using var stream = new FileStream(avatarFilePath, FileMode.Create);
        await NewAvatar.CopyToAsync(stream);

        TempData["SuccessMessage"] = "Avatar updated successfully!";
        return RedirectToAction("Settings");
    }


    // Courses List with search, filter, sort and paging
    public IActionResult Courses(
    string search,
    decimal? minPrice,
    decimal? maxPrice,
    string sortBy,
    string sortOrder,
    int page = 1,
    string viewMode = "table") //New viewMode parameter, default table
    {
        int pageSize = 20;

        var courses = _context.Courses.AsQueryable();

        // Search: Title and description
        if (!string.IsNullOrEmpty(search))
        {
            courses = courses.Where(c =>
                (c.Title ?? "").Contains(search) ||
                (c.Description ?? "").Contains(search));
        }

        // Price range selection
        if (minPrice.HasValue)
            courses = courses.Where(c => c.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            courses = courses.Where(c => c.Price <= maxPrice.Value);

        // Sorting logic
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
            _ => courses.OrderByDescending(c => c.CreatedAt)
        };

        // Paging computation
        int totalItems = courses.Count();
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        // Make sure the page number is valid
        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages > 0 ? totalPages : 1;

        var pagedCourses = courses
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        // Preserve query criteria and sorting parameters for paging and views
        ViewBag.CurrentSearch = search ?? "";
        ViewBag.CurrentMinPrice = minPrice?.ToString() ?? "";
        ViewBag.CurrentMaxPrice = maxPrice?.ToString() ?? "";
        ViewBag.CurrentSortBy = sortBy ?? "";
        ViewBag.CurrentSortOrder = sortOrder ?? "";
        ViewBag.ViewMode = viewMode ?? "table"; // Save viewMode

        return View(pagedCourses);
    }


    [HttpGet]
    public IActionResult CreateCourse()
    {
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> CreateCourse(
        Course model,
        List<IFormFile> UploadedVideos,
        List<string> VideoNames,
        IFormFile CoverImage)
    {
        // Basic checks
        if (!ModelState.IsValid)
            return View(model);

        if (model.VideoCount != UploadedVideos.Count)
        {
            ModelState.AddModelError("", "Video count does not match number of uploaded files.");
            return View(model);
        }

        model.CreatedAt = DateTime.Now;

        // Save initial course record (used to generate course ID)
        _context.Courses.Add(model);
        await _context.SaveChangesAsync();

        // Handle the cover image upload
        if (CoverImage != null && CoverImage.Length > 0)
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "courseimages");
            if (!Directory.Exists(imagePath))
                Directory.CreateDirectory(imagePath);

            var imageExt = Path.GetExtension(CoverImage.FileName);
            var imageFileName = $"{model.Id}_cover{imageExt}";
            var imageFullPath = Path.Combine(imagePath, imageFileName);

            using (var stream = new FileStream(imageFullPath, FileMode.Create))
            {
                await CoverImage.CopyToAsync(stream);
            }

            model.CoverImagePath = $"/courseimages/{imageFileName}";
        }

        // Handle video file uploads
        var videoDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Video");
        if (!Directory.Exists(videoDir))
            Directory.CreateDirectory(videoDir);

        var displayPairs = new List<string>();
        var fileNamesOnly = new List<string>();

        for (int i = 0; i < UploadedVideos.Count; i++)
        {
            var file = UploadedVideos[i];
            var displayName = (VideoNames != null && VideoNames.Count > i && !string.IsNullOrWhiteSpace(VideoNames[i]))
                ? VideoNames[i]
                : $"默认视频-{i + 1}";

            if (file != null && file.Length > 0)
            {
                var ext = Path.GetExtension(file.FileName);
                var fileName = $"{model.Id}_video_{i + 1}{ext}";
                var fullPath = Path.Combine(videoDir, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                displayPairs.Add($"{fileName}|{displayName}");
                fileNamesOnly.Add(fileName);
            }
        }

        // Set the video field for the course
        model.VideoFileNames = string.Join(";", displayPairs);
        model.VideoFilePaths = string.Join(";", fileNamesOnly);

        // Update course record (cover image & video field)
        _context.Courses.Update(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Courses");
    }



    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Courses");
    }

    // GET: /Account/EditCourse/5
    [HttpGet]
    // GET: /Account/EditCourse/5
    public async Task<IActionResult> EditCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    // POST: /Account/EditCourse
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(
    [Bind("Id,Title,Description,Price,VideoCount")] Learnhub.Models.Course model,
    IFormFile CoverImage,
    List<IFormFile>? UploadedVideos,
    List<string>? VideoNames)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Validation failed.");
            return View(model);
        }

        var course = await _context.Courses.FindAsync(model.Id);
        if (course == null)
        {
            return NotFound();
        }

        // Update basic information
        course.Title = model.Title;
        course.Description = model.Description;
        course.Price = model.Price;
        course.VideoCount = model.VideoCount;

        // Save the cover image
        if (CoverImage != null && CoverImage.Length > 0)
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "courseimages");
            Directory.CreateDirectory(imagePath);

            var imageExt = Path.GetExtension(CoverImage.FileName);
            var imageFileName = $"{course.Id}_cover{imageExt}";
            var imageFullPath = Path.Combine(imagePath, imageFileName);

            using (var stream = new FileStream(imageFullPath, FileMode.Create))
            {
                await CoverImage.CopyToAsync(stream);
            }

            course.CoverImagePath = $"/courseimages/{imageFileName}";
        }

        // Saving the video file
        if (UploadedVideos != null && UploadedVideos.Count > 0)
        {
            var videoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Video");
            Directory.CreateDirectory(videoPath);

            var fileNameDisplayPairs = new List<string>();
            var filePaths = new List<string>();

            for (int i = 0; i < UploadedVideos.Count; i++)
            {
                var file = UploadedVideos[i];
                var displayName = (VideoNames != null && VideoNames.Count > i && !string.IsNullOrWhiteSpace(VideoNames[i]))
                    ? VideoNames[i]
                    : $"默认视频-{i + 1}";

                if (file != null && file.Length > 0)
                {
                    var ext = Path.GetExtension(file.FileName);
                    var fileName = $"{course.Id}_video_{i + 1}{ext}";
                    var fullPath = Path.Combine(videoPath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    fileNameDisplayPairs.Add($"{fileName}|{displayName}");
                    filePaths.Add($"/Video/{fileName}");
                }
            }

            course.VideoFileNames = string.Join(";", fileNameDisplayPairs);
            course.VideoFilePaths = string.Join(";", filePaths);
        }

        try
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Course updated successfully.";
            return RedirectToAction("Courses", "Account");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Failed to update course: {ex.Message}");
            return View(model);
        }
    }


    public async Task<IActionResult> PlayCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }


    public async Task<IActionResult> Users(
        string search,
        decimal? minWallet,
        decimal? maxWallet,
        string sortBy,
        string sortOrder,
        int page = 1,
        int pageSize = 20)
    {
        var users = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            string pattern = $"%{search}%";

            users = users.Where(u =>
                EF.Functions.Like(u.Username, pattern) ||
                EF.Functions.Like(u.Email, pattern) ||
                EF.Functions.Like(u.Role, pattern));
        }

        if (minWallet.HasValue)
            users = users.Where(u => u.WalletBalance >= minWallet.Value);
        if (maxWallet.HasValue)
            users = users.Where(u => u.WalletBalance <= maxWallet.Value);

        bool isDescending = sortOrder == "desc";
        users = sortBy switch
        {
            "Username" => isDescending ? users.OrderByDescending(u => u.Username) : users.OrderBy(u => u.Username),
            "Email" => isDescending ? users.OrderByDescending(u => u.Email) : users.OrderBy(u => u.Email),
            "WalletBalance" => isDescending ? users.OrderByDescending(u => u.WalletBalance) : users.OrderBy(u => u.WalletBalance),
            "Id" => isDescending ? users.OrderByDescending(u => u.Id) : users.OrderBy(u => u.Id),
            _ => users.OrderBy(u => u.Id)
        };

        int totalItems = await users.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages > 0 ? totalPages : 1;

        var pagedUsers = await users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.CurrentSearch = search ?? "";
        ViewBag.CurrentMinWallet = minWallet?.ToString() ?? "";
        ViewBag.CurrentMaxWallet = maxWallet?.ToString() ?? "";
        ViewBag.CurrentSortBy = sortBy ?? "";
        ViewBag.CurrentSortOrder = sortOrder ?? "";

        return View(pagedUsers);
    }




    // Displays the Create user page
    public IActionResult CreateUser()
    {
        return View();
    }

    // Submit the Create user form
    [HttpPost]
    public async Task<IActionResult> CreateUser(User user)
    {
        if (ModelState.IsValid)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Users");
        }
        return View(user);
    }

    //Display the edit page
    public async Task<IActionResult> EditUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        return View(user);
    }

    // Save the edited data
    [HttpPost]
    public async Task<IActionResult> EditUser(User user)
    {
        if (ModelState.IsValid)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Users");
        }
        return View(user);
    }

    //Deleting users
    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Users");
    }






    [HttpPost]
    public async Task<IActionResult> LikeCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return NotFound();

        course.Likes += 1;
        await _context.SaveChangesAsync();

        return Json(new { likes = course.Likes });
    }


    [HttpPost]
    public async Task<IActionResult> AddView(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return NotFound();

        course.Views += 1;
        await _context.SaveChangesAsync();

        return Ok();
    }


    public async Task<IActionResult> ActivationCodes(string? search, string? sortBy, string? sortOrder, int page = 1)
    {
        int pageSize = 10;

        var query = _context.AdminActivationCodes.Include(a => a.UsedByUser).AsQueryable();

        // Handle search criteria safely (avoid null references)
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a =>
                a.ActivationCode.Contains(search) ||
                a.Status.Contains(search) ||
                (a.UsedByUser != null && a.UsedByUser.Username.Contains(search))); // A null check is added
        }

        // Sorting logic
        ViewBag.CurrentSortBy = sortBy;
        ViewBag.CurrentSortOrder = sortOrder;
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentPage = page;

        query = sortBy switch
        {
            "ActivationCode" => sortOrder == "desc"
                ? query.OrderByDescending(a => a.ActivationCode)
                : query.OrderBy(a => a.ActivationCode),

            "Status" => sortOrder == "desc"
                ? query.OrderByDescending(a => a.Status)
                : query.OrderBy(a => a.Status),

            "Username" => sortOrder == "desc"
                ? query.OrderByDescending(a => a.UsedByUser != null ? a.UsedByUser.Username : "") // Add default values
                : query.OrderBy(a => a.UsedByUser != null ? a.UsedByUser.Username : ""),          // Add default values

            _ => query.OrderBy(a => a.Id)
        };

        // pagination
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.TotalPages = totalPages;

        var result = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(result);
    }


    // Add actions supported by the view
    public IActionResult CreateActivationCode() => View();

    [HttpPost]
    public async Task<IActionResult> CreateActivationCode(AdminActivationCode model)
    {
        if (ModelState.IsValid)
        {
            _context.AdminActivationCodes.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("ActivationCodes");
        }
        return View(model);
    }

    public async Task<IActionResult> EditActivationCode(int id)
    {
        var code = await _context.AdminActivationCodes.FindAsync(id);
        if (code == null) return NotFound();
        return View(code);
    }

    [HttpPost]
    public async Task<IActionResult> EditActivationCode(AdminActivationCode model)
    {
        if (ModelState.IsValid)
        {
            _context.AdminActivationCodes.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("ActivationCodes");
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteActivationCode(int id)
    {
        var code = await _context.AdminActivationCodes.FindAsync(id);
        if (code != null)
        {
            _context.AdminActivationCodes.Remove(code);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("ActivationCodes");
    }

    //The newly added code
    [HttpPost]
    public async Task<IActionResult> ActivateRechargeCode(string rechargeCode)
    {
        var userId = GetCurrentUserId();

        // Null-safety check on user
        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            TempData["RechargeMessage"] = "The user does not exist.";
            return RedirectToAction("UserProfile");
        }

        // Null-safety check on activation code
        var code = await _context.WalletActivationCodes
            .FirstOrDefaultAsync(c => c.ActivationCode == rechargeCode && c.Status == "unused");

        if (code is null)
        {
            TempData["RechargeMessage"] = "The activation code is invalid or has been used.";
            return RedirectToAction("UserProfile");
        }

        //  Update wallet balance
        user.WalletBalance += code.Amount;

        //  Update activation code information
        code.Status = "used";
        code.UsedByUserId = user.Id;
        code.UsedOn = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["RechargeMessage"] = $"Recharge successful. Your wallet has been increased {code.Amount} ";
        return RedirectToAction("UserProfile");
    }

    // Securely obtain the current user ID

    private int GetCurrentUserId()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedId))
        {
            throw new Exception("Unable to obtain valid user information ID");
        }
        return parsedId;
    }



    public async Task<IActionResult> UserProfile()
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users.FindAsync(userId);

        //  Add null check
        if (user is null)
        {
            TempData["RechargeMessage"] = "Unable to load user information";
            return RedirectToAction("Login"); // Or Error Page
        }

        return View(user);
    }



    //The code block for managing wallet activation codes
    // Wallet activation code page + search + sorting logic
    public async Task<IActionResult> WalletActivationCodes(string? search, string? sortBy, string? sortOrder, int page = 1)
    {
        int pageSize = 10;
        var query = _context.WalletActivationCodes.Include(c => c.UsedByUser).AsQueryable();

        // Search processing
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c =>
                (c.ActivationCode != null && c.ActivationCode.Contains(search)) ||
                (c.Status != null && c.Status.Contains(search)) ||
                (c.UsedByUser != null && c.UsedByUser.Username != null && c.UsedByUser.Username.Contains(search))
            );
        }

        // Sorting parameters are stored for view display.
        ViewBag.CurrentSortBy = sortBy;
        ViewBag.CurrentSortOrder = sortOrder;
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentPage = page;

        // Sorting logic
        query = sortBy switch
        {
            "ActivationCode" => sortOrder == "desc"
                ? query.OrderByDescending(c => c.ActivationCode ?? "")
                : query.OrderBy(c => c.ActivationCode ?? ""),

            "Status" => sortOrder == "desc"
                ? query.OrderByDescending(c => c.Status ?? "")
                : query.OrderBy(c => c.Status ?? ""),

            "Username" => sortOrder == "desc"
                ? query.OrderByDescending(c => c.UsedByUser != null ? c.UsedByUser.Username ?? "" : "")
                : query.OrderBy(c => c.UsedByUser != null ? c.UsedByUser.Username ?? "" : ""),

            _ => query.OrderBy(c => c.Id)
        };

        // Pagination
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.TotalPages = totalPages;

        var result = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(result);
    }



public IActionResult CreateWalletActivationCode() => View();

    [HttpPost]
    public async Task<IActionResult> CreateWalletActivationCode(WalletActivationCode model)
    {
        if (ModelState.IsValid)
        {
            _context.WalletActivationCodes.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("WalletActivationCodes");
        }
        return View(model);
    }



public async Task<IActionResult> EditWalletActivationCode(int id)
    {
        var code = await _context.WalletActivationCodes.FindAsync(id);
        if (code is null) return NotFound();
        return View(code);
    }

    [HttpPost]
    public async Task<IActionResult> EditWalletActivationCode(WalletActivationCode model)
    {
        if (ModelState.IsValid)
        {
            _context.WalletActivationCodes.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("WalletActivationCodes");
        }
        return View(model);
    }



[HttpPost]
    public async Task<IActionResult> DeleteWalletActivationCode(int id)
    {
        var code = await _context.WalletActivationCodes.FindAsync(id);
        if (code != null)
        {
            _context.WalletActivationCodes.Remove(code);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("WalletActivationCodes");
    }




}











