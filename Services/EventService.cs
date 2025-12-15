using GuildManagement.Data;
using GuildManagement.DTOs;
using GuildManagement.Entities;
using GuildManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GuildManagement.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IAchievementRepository _achievementRepository;
        private readonly GuildManagementContext _context;

        public EventService(IEventRepository eventRepository, IAchievementRepository achievementRepository, GuildManagementContext context)
        {
            _eventRepository = eventRepository;
            _achievementRepository = achievementRepository;
            _context = context;
        }

        public async Task<List<Event>> GetAllEvents()
        {
            return (await _eventRepository.GetAll()).ToList();
        }

        public async Task<Event?> GetEventById(int id)
        {
            return await _eventRepository.GetById(id);
        }

        public async Task<Event> CreateEvent(Event @event)
        {
            if (@event.StartDate < DateTime.Now)
            {
                throw new ArgumentException("Дата события не может быть в прошлом");
            }
            return await _eventRepository.Add(@event);
        }

        public async Task<Event> UpdateEvent(int id, Event @event)
        {
            var existingEvent = await _eventRepository.GetById(id);
            if (existingEvent == null)
            {
                throw new ArgumentException("Событие не найдено");
            }
            existingEvent.Title = @event.Title;
            existingEvent.Description = @event.Description;
            existingEvent.Type = @event.Type;
            existingEvent.StartDate = @event.StartDate;
            existingEvent.EndDate = @event.EndDate;

            return await _eventRepository.Update(existingEvent);
        }

        public async Task<bool> DeleteEvent(int id)
        {
            try
            {
                await _eventRepository.Delete(id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Event>> GetEventsByTitle(string title)
        {
            return (await _eventRepository.GetByTitle(title)).ToList();
        }

        public async Task<List<Event>> GetEventsByMemberId(int memberId)
        {
            return (await _eventRepository.GetByMemberId(memberId)).ToList();
        }

        public async Task<List<Event>> GetEventsByType(EventType type)
        {
            return (await _eventRepository.GetByType(type)).ToList();
        }

        public async Task<List<Event>> GetUpcomingEvents()
        {
            return (await _eventRepository.GetUpcomingEvents()).ToList();
        }

        public async Task<List<Event>> GetEventsByDateRange(DateTime start, DateTime end)
        {
            if (start > end)
            {
                throw new ArgumentException("Начальная дата не может быть больше конечной");
            }

            return (await _eventRepository.GetByDateRange(start, end)).ToList();
        }

        public async Task<bool> CompleteEvent(int eventId, Achievement? achievement = null)
        {
            int? achievementId = null;
            if (achievement != null)
            {
                var createdAchievement = await _achievementRepository.Add(achievement);
                achievementId = createdAchievement.Id;
            }

            return await _eventRepository.CompleteEvent(eventId, achievementId);
        }

        //новое
        public async Task<List<EventDTO>> GetAllEventsDTO()
        {
            var events = await _context.Events
                .Include(e => e.Member)
                .Include(e => e.Achievement)
                .ToListAsync();

            return events.Select(ConvertToDTO).ToList();
        }

        public async Task<EventDTO?> GetEventDTOById(int id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Member)
                .Include(e => e.Achievement)
                .FirstOrDefaultAsync(e => e.Id == id);

            return eventEntity != null ? ConvertToDTO(eventEntity) : null;
        }

        public async Task<EventDTO> CreateEventDTO(CreateEventDTO eventDto)
        {
            var member = await _context.Members.FindAsync(eventDto.MemberId);
            if (member == null)
            {
                throw new ArgumentException("Участник не найден");
            }

            if (!Enum.TryParse<EventType>(eventDto.Type, out var eventType))
            {
                throw new ArgumentException("Неверный тип события");
            }

            var eventEntity = new Event
            {
                Title = eventDto.Title,
                Description = eventDto.Description,
                Type = eventType,
                StartDate = eventDto.StartDate,
                EndDate = eventDto.StartDate.AddHours(2),
                MemberId = eventDto.MemberId
            };
            var createdEvent = await _eventRepository.Add(eventEntity);

            return ConvertToDTO(createdEvent);
        }

        public async Task<EventDTO> UpdateEventDTO(int id, CreateEventDTO eventDto)
        {
            var existingEvent = await _eventRepository.GetById(id);
            if (existingEvent == null)
            {
                throw new ArgumentException("Событие не найдено");
            }

            var member = await _context.Members.FindAsync(eventDto.MemberId);
            if (member == null)
            {
                throw new ArgumentException("Участник не найден");
            }

            if (!Enum.TryParse<EventType>(eventDto.Type, out var eventType))
            {
                throw new ArgumentException("Неверный тип события");
            }

            existingEvent.Title = eventDto.Title;
            existingEvent.Description = eventDto.Description;
            existingEvent.Type = eventType;
            existingEvent.StartDate = eventDto.StartDate;
            existingEvent.EndDate = eventDto.StartDate.AddHours(2);
            existingEvent.MemberId = eventDto.MemberId;
            var updatedEvent = await _eventRepository.Update(existingEvent);

            return ConvertToDTO(updatedEvent);
        }
        public async Task<List<EventDTO>> GetEventsByMemberIdDTO(int memberId)
        {
            var events = await _context.Events
                .Where(e => e.MemberId == memberId)
                .Include(e => e.Member)
                .ToListAsync();

            return events.Select(ConvertToDTO).ToList();
        }

        public async Task<List<EventDTO>> GetUpcomingEventsDTO()
        {
            var events = await _context.Events
                .Where(e => e.StartDate > DateTime.Now)
                .Include(e => e.Member)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return events.Select(ConvertToDTO).ToList();
        }

        public async Task<List<EventDTO>> GetEventsByTypeDTO(string eventType)
        {
            if (!Enum.TryParse<EventType>(eventType, out var eventTypeEnum))
            {
                throw new ArgumentException("Неверный тип события");
            }

            var events = await _context.Events
                .Where(e => e.Type == eventTypeEnum)
                .Include(e => e.Member)
                .ToListAsync();

            return events.Select(ConvertToDTO).ToList();
        }

        private EventDTO ConvertToDTO(Event eventEntity)
        {
            return new EventDTO
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
        }
    }
}