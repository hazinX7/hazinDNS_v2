using System.ComponentModel.DataAnnotations;

namespace hazinDNS_v2.Models
{
    public class UpdateBalanceModel
    {
        [Required(ErrorMessage = "Введите сумму пополнения")]
        [Range(1, 750000, ErrorMessage = "Сумма пополнения должна быть от 1 до 750 000 рублей")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Введите номер карты")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Номер карты должен содержать 16 цифр")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Введите срок действия карты")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([2-9]\d)$", ErrorMessage = "Введите корректный срок действия карты (ММ/ГГ)")]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "Введите CVV код")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV код должен содержать 3 цифры")]
        public string CVV { get; set; }
    }
} 