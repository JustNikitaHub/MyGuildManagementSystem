using GuildManagement.Entities;
using GuildManagement.Interfaces;

namespace GuildManagement.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IAchievementRepository _achievementRepository;

        public EventService(IEventRepository eventRepository, IAchievementRepository achievementRepository)
        {
            _eventRepository = eventRepository;
            _achievementRepository = achievementRepository;
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
    }
}