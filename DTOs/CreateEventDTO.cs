using System.ComponentModel.DataAnnotations;

namespace GuildManagement.DTOs
{
    public class CreateEventDTO
    {
        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 100 символов")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Тип события обязателен")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Дата начала обязательна")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "ID участника обязателен")]
        public int MemberId { get; set; }
    }
}