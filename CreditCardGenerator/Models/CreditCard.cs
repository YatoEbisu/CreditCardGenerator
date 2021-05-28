using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CreditCardGenerator.Models
{
    public class CreditCard
    {
        [Key]
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Incorrect email format")]
        [Required(ErrorMessage = "required field")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("credit_card_number")]
        public long? CreditCardNumber { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}