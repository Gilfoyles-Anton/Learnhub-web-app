using System.ComponentModel.DataAnnotations;

namespace Learnhub.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password cannot be empty")]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the confirmation password")]
        [DataType(DataType.Password)]
        [Compare("PasswordHash", ErrorMessage = "The two passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPasswordHash { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = "User";

        [Display(Name = "Admin Activation Code")]
        public string? AdminActivationCode { get; set; }

        // New wallet balance field, default 0
        [Display(Name = "Wallet Balance")]
        [Range(0, double.MaxValue, ErrorMessage = "Wallet balance must be non-negative")]
        public decimal WalletBalance { get; set; } = 0m;

        public string? BackgroundImagePath { get; set; }
    }
}



