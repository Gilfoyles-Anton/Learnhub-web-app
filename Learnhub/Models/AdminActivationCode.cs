using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnhub.Models
{
    public class AdminActivationCode
    {
        [Key]
        public int Id { get; set; }

        public int CodeId { get; set; }

        [Required]
        public required string ActivationCode { get; set; }

        [Required]
        public required string Status { get; set; } = "unused";

        public int? UsedByUserId { get; set; }

        [ForeignKey("UsedByUserId")]
        public User? UsedByUser { get; set; }
    }
}
