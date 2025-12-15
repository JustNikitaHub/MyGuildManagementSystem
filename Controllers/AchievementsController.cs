using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GuildManagement.Data;
using GuildManagement.Entities;
using GuildManagement.DTOs;

namespace GuildManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AchievementsController : ControllerBase
    {
        private readonly GuildManagementContext _context;

        public AchievementsController(GuildManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AchievementDTO>>> GetAchievements()
        {
            var achievements = await _context.Achievements
                .Include(a => a.Member)
                .Include(a => a.Event)
                .ToListAsync();

            var achievementDTOs = achievements.Select(a => new AchievementDTO
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                EarnedDate = a.EarnedDate,
                MemberId = a.MemberId,
                MemberName = a.Member?.Name,
                EventId = a.EventId,
                EventTitle = a.Event?.Title
            }).ToList();

            return Ok(achievementDTOs);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AchievementDTO>> GetAchievement(int id)
        {
            var achievement = await _context.Achievements
                .Include(a => a.Member)
                .Include(a => a.Event)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (achievement == null)
            {
                return NotFound();
            }

            var achievementDTO = new AchievementDTO
            {
                Id = achievement.Id,
                Title = achievement.Title,
                Description = achievement.Description,
                EarnedDate = achievement.EarnedDate,
                MemberId = achievement.MemberId,
                MemberName = achievement.Member?.Name,
                EventId = achievement.EventId,
                EventTitle = achievement.Event?.Title
            };

            return Ok(achievementDTO);
        }

        [HttpPost]
        public async Task<ActionResult<AchievementDTO>> PostAchievement(CreateAchievementDTO createAchievementDTO)
        {
            var member = await _context.Members.FindAsync(createAchievementDTO.MemberId);
            if (member == null)
            {
                return BadRequest("Участник не найден");
            }
            if (createAchievementDTO.EventId.HasValue)
            {
                var eventEntity = await _context.Events.FindAsync(createAchievementDTO.EventId.Value);
                if (eventEntity == null)
                {
                    return BadRequest("Событие не найдено");
                }
            }

            var achievement = new Achievement
            {
                Title = createAchievementDTO.Title,
                Description = createAchievementDTO.Description,
                EarnedDate = DateTime.UtcNow,
                MemberId = createAchievementDTO.MemberId,
                EventId = createAchievementDTO.EventId
            };

            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();

            var achievementDTO = new AchievementDTO
            {
                Id = achievement.Id,
                Title = achievement.Title,
                Description = achievement.Description,
                EarnedDate = achievement.EarnedDate,
                MemberId = achievement.MemberId,
                MemberName = member.Name,
                EventId = achievement.EventId
            };

            return CreatedAtAction(nameof(GetAchievement), new { id = achievement.Id }, achievementDTO);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAchievement(int id, CreateAchievementDTO createAchievementDTO)
        {
            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement == null)
            {
                return NotFound();
            }
            var member = await _context.Members.FindAsync(createAchievementDTO.MemberId);
            if (member == null)
            {
                return BadRequest("Участник не найден");
            }
            if (createAchievementDTO.EventId.HasValue)
            {
                var eventEntity = await _context.Events.FindAsync(createAchievementDTO.EventId.Value);
                if (eventEntity == null)
                {
                    return BadRequest("Событие не найдено");
                }
            }

            achievement.Title = createAchievementDTO.Title;
            achievement.Description = createAchievementDTO.Description;
            achievement.MemberId = createAchievementDTO.MemberId;
            achievement.EventId = createAchievementDTO.EventId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AchievementExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAchievement(int id)
        {
            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement == null)
            {
                return NotFound();
            }

            _context.Achievements.Remove(achievement);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<IEnumerable<AchievementDTO>>> GetAchievementsByMember(int memberId)
        {
            var achievements = await _context.Achievements
                .Where(a => a.MemberId == memberId)
                .Include(a => a.Event)
                .OrderByDescending(a => a.EarnedDate)
                .ToListAsync();

            var achievementDTOs = achievements.Select(a => new AchievementDTO
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                EarnedDate = a.EarnedDate,
                MemberId = a.MemberId,
                EventId = a.EventId,
                EventTitle = a.Event?.Title
            }).ToList();

            return Ok(achievementDTOs);
        }

        private bool AchievementExists(int id)
        {
            return _context.Achievements.Any(e => e.Id == id);
        }
    }
}