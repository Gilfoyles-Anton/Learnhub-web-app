using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Learnhub.Models
{
    public class UserSettingsViewModel
    {
        [Required]
        [Display(Name = "Old Email")]
        public string OldEmail { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Old Phone Number")]
        public string OldPhone { get; set; } = string.Empty;

        [Display(Name = "New Username")]
        public string? NewUsername { get; set; }

        [Display(Name = "New Email")]
        [EmailAddress]
        public string? NewEmail { get; set; }

        [Display(Name = "New Phone")]
        [Phone]
        public string? NewPhone { get; set; }

        [Display(Name = "Current Password")]
        public string OldPassword { get; set; } = string.Empty;

        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [Display(Name = "Upload New Avatar")]
        public IFormFile? NewAvatar { get; set; }
    }
}

