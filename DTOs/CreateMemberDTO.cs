using System.ComponentModel.DataAnnotations;

namespace GuildManagement.DTOs
{
    public class CreateMemberDTO
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 50 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Уровень обязателен")]
        [Range(1, 60, ErrorMessage = "Уровень должен быть от 1 до 60")]
        public int Level { get; set; }

        [Required(ErrorMessage = "Класс обязателен")]
        public string MemberClass { get; set; } = string.Empty;
    }
}