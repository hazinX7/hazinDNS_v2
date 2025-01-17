using System.ComponentModel.DataAnnotations;

namespace hazinDNS_v2.Models
{
    public class UpdateBalanceModel
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal NewBalance { get; set; }
    }
} 