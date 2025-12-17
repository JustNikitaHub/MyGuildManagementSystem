using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GuildManagement.Data;
using GuildManagement.Entities;
using GuildManagement.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace GuildManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MembersController : ControllerBase
    {
        private readonly GuildManagementContext _context;

        public MembersController(GuildManagementContext context)
        {
            _context = context;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetMembers()
        {
            var members = await _context.Members
                .Include(m => m.Events)
                .Include(m => m.Achievements)
                .Include(m => m.Resources)
                .ToListAsync();

            var memberDTOs = members.Select(m => new MemberDTO
            {
                Id = m.Id,
                Name = m.Name,
                Level = m.Level,
                MemberClass = m.MemberClass.ToString(),
                EventIds = m.Events.Select(e => e.Id).ToList(),
                AchievementIds = m.Achievements.Select(a => a.Id).ToList(),
                ResourceIds = m.Resources.Select(r => r.Id).ToList(),
                EventsCount = m.Events.Count,
                AchievementsCount = m.Achievements.Count,
                ResourcesCount = m.Resources.Count
            }).ToList();

            return Ok(memberDTOs);
        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<MemberDTO>> GetMember(int id)
        {
            var member = await _context.Members
                .Include(m => m.Events)
                .Include(m => m.Achievements)
                .Include(m => m.Resources)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return NotFound();
            }

            var memberDTO = new MemberDTO
            {
                Id = member.Id,
                Name = member.Name,
                Level = member.Level,
                MemberClass = member.MemberClass.ToString(),
                EventIds = member.Events.Select(e => e.Id).ToList(),
                AchievementIds = member.Achievements.Select(a => a.Id).ToList(),
                ResourceIds = member.Resources.Select(r => r.Id).ToList(),
                EventsCount = member.Events.Count,
                AchievementsCount = member.Achievements.Count,
                ResourcesCount = member.Resources.Count
            };

            return Ok(memberDTO);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<MemberDTO>> PostMember(CreateMemberDTO createMemberDTO)
        {
            if (!Enum.TryParse<MemberClass>(createMemberDTO.MemberClass, out var memberClass))
            {
                return BadRequest("Неверный класс участника");
            }

            var member = new Member
            {
                Name = createMemberDTO.Name,
                Level = createMemberDTO.Level,
                MemberClass = memberClass
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var memberDTO = new MemberDTO
            {
                Id = member.Id,
                Name = member.Name,
                Level = member.Level,
                MemberClass = member.MemberClass.ToString()
            };

            return CreatedAtAction(nameof(GetMember), new { id = member.Id }, memberDTO);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> PutMember(int id, CreateMemberDTO createMemberDTO)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            if (!Enum.TryParse<MemberClass>(createMemberDTO.MemberClass, out var memberClass))
            {
                return BadRequest("Неверный класс участника");
            }

            member.Name = createMemberDTO.Name;
            member.Level = createMemberDTO.Level;
            member.MemberClass = memberClass;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
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
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("class/{memberClass}")]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetMembersByClass(string memberClass)
        {
            if (!Enum.TryParse<MemberClass>(memberClass, out var memberClassEnum))
            {
                return BadRequest("Неверный класс участника");
            }

            var members = await _context.Members
                .Where(m => m.MemberClass == memberClassEnum)
                .ToListAsync();

            var memberDTOs = members.Select(m => new MemberDTO
            {
                Id = m.Id,
                Name = m.Name,
                Level = m.Level,
                MemberClass = m.MemberClass.ToString()
            }).ToList();

            return Ok(memberDTOs);
        }
        [HttpPut("{id}/level")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ChangeMemberLevel(int id, [FromBody] int newLevel)
        {
            if (newLevel < 1 || newLevel > 60)
            {
                return BadRequest("Уровень должен быть от 1 до 60");
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            member.Level = newLevel;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.Id == id);
        }
    }
}