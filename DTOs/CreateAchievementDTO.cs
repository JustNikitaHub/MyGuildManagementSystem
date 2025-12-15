using System.ComponentModel.DataAnnotations;

namespace GuildManagement.DTOs
{
    public class CreateAchievementDTO
    {
        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 100 символов")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Описание обязательно")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Описание должно быть от 10 до 500 символов")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID участника обязателен")]
        public int MemberId { get; set; }
        public int? EventId { get; set; }
    }
}