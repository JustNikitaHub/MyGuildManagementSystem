using System.ComponentModel.DataAnnotations;

namespace GuildManagement.DTOs
{
    public class CreateResourceDTO
    {
        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Название должно быть от 2 до 100 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Тип обязателен")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Количество обязательно")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть положительным числом")]
        public int Quantity { get; set; }

        public string? Rarity { get; set; }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "ID участника обязателен")]
        public int MemberId { get; set; }
    }
}