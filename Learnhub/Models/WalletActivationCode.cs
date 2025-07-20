using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnhub.Models
{
    public class WalletActivationCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CodeId { get; set; }

        [Required]
        [StringLength(50)]
        public string? ActivationCode { get; set; }  //  Marked as nullable

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(10)]
        public string Status { get; set; } = "unused";

        public int? UsedByUserId { get; set; }

        [ForeignKey("UsedByUserId")]
        public virtual User? UsedByUser { get; set; }  //  Marked as nullable

        public DateTime? UsedOn { get; set; }
    }
}


