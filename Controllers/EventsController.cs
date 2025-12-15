using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GuildManagement.Data;
using GuildManagement.Entities;
using GuildManagement.DTOs;

namespace GuildManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly GuildManagementContext _context;

        public EventsController(GuildManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.Member)
                .Include(e => e.Achievement)
                .ToListAsync();

            var eventDTOs = events.Select(e => new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Type = e.Type.ToString(),
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                MemberId = e.MemberId,
                MemberName = e.Member?.Name,
                AchievementId = e.Achievement?.Id,
                AchievementTitle = e.Achievement?.Title
            }).ToList();

            return Ok(eventDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventDTO>> GetEvent(int id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Member)
                .Include(e => e.Achievement)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            var eventDTO = new EventDTO
            {
                Id = eventEntity.Id,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                Type = eventEntity.Type.ToString(),
                StartDate = eventEntity.StartDate,
                EndDate = eventEntity.EndDate,
                MemberId = eventEntity.MemberId,
                MemberName = eventEntity.Member?.Name,
                AchievementId = eventEntity.Achievement?.Id,
                AchievementTitle = eventEntity.Achievement?.Title
            };

            return Ok(eventDTO);
        }

        [HttpPost]
        public async Task<ActionResult<EventDTO>> PostEvent(CreateEventDTO createEventDTO)
        {
            var member = await _context.Members.FindAsync(createEventDTO.MemberId);
            if (member == null)
            {
                return BadRequest("Участник не найден");
            }

            if (!Enum.TryParse<EventType>(createEventDTO.Type, out var eventType))
            {
                return BadRequest("Неверный тип события");
            }

            var eventEntity = new Event
            {
                Title = createEventDTO.Title,
                Description = createEventDTO.Description,
                Type = eventType,
                StartDate = createEventDTO.StartDate,
                EndDate = createEventDTO.StartDate.AddHours(2),
                MemberId = createEventDTO.MemberId
            };

            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var eventDTO = new EventDTO
            {
                Id = eventEntity.Id,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                Type = eventEntity.Type.ToString(),
                StartDate = eventEntity.StartDate,
                EndDate = eventEntity.EndDate,
                MemberId = eventEntity.MemberId,
                MemberName = member.Name
            };

            return CreatedAtAction(nameof(GetEvent), new { id = eventEntity.Id }, eventDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, CreateEventDTO createEventDTO)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(createEventDTO.MemberId);
            if (member == null)
            {
                return BadRequest("Участник не найден");
            }

            if (!Enum.TryParse<EventType>(createEventDTO.Type, out var eventType))
            {
                return BadRequest("Неверный тип события");
            }

            eventEntity.Title = createEventDTO.Title;
            eventEntity.Description = createEventDTO.Description;
            eventEntity.Type = eventType;
            eventEntity.StartDate = createEventDTO.StartDate;
            eventEntity.EndDate = createEventDTO.StartDate.AddHours(2);
            eventEntity.MemberId = createEventDTO.MemberId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
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
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetUpcomingEvents()
        {
            var events = await _context.Events
                .Where(e => e.StartDate > DateTime.Now)
                .Include(e => e.Member)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            var eventDTOs = events.Select(e => new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Type = e.Type.ToString(),
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                MemberId = e.MemberId,
                MemberName = e.Member?.Name
            }).ToList();

            return Ok(eventDTOs);
        }

        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEventsByMember(int memberId)
        {
            var events = await _context.Events
                .Where(e => e.MemberId == memberId)
                .Include(e => e.Member)
                .ToListAsync();

            var eventDTOs = events.Select(e => new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Type = e.Type.ToString(),
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                MemberId = e.MemberId,
                MemberName = e.Member?.Name
            }).ToList();

            return Ok(eventDTOs);
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}