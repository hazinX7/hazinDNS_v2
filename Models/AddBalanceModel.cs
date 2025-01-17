using System.ComponentModel.DataAnnotations;

namespace hazinDNS_v2.Models
{
    public class AddBalanceModel
    {
        [Required]
        [Range(1, 750000)]
        public decimal Amount { get; set; }
    }
} 