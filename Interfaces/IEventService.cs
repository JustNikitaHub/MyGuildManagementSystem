using GuildManagement.DTOs;
using GuildManagement.Entities;

namespace GuildManagement.Interfaces
{
    public interface IEventService
    {
        Task<List<Event>> GetAllEvents();
        Task<Event?> GetEventById(int id);
        Task<Event> CreateEvent(Event @event);
        Task<Event> UpdateEvent(int id, Event @event);
        Task<bool> DeleteEvent(int id);
        Task<List<Event>> GetEventsByTitle(string title);
        Task<List<Event>> GetEventsByMemberId(int memberId);
        Task<List<Event>> GetEventsByType(EventType type);
        Task<List<Event>> GetUpcomingEvents();
        Task<List<Event>> GetEventsByDateRange(DateTime start, DateTime end);
        Task<bool> CompleteEvent(int eventId, Achievement? achievement = null);

        //новое
        Task<List<EventDTO>> GetAllEventsDTO();
        Task<EventDTO?> GetEventDTOById(int id);
        Task<EventDTO> CreateEventDTO(CreateEventDTO eventDto);
        Task<EventDTO> UpdateEventDTO(int id, CreateEventDTO eventDto);
        Task<List<EventDTO>> GetEventsByMemberIdDTO(int memberId);
        Task<List<EventDTO>> GetUpcomingEventsDTO();
        Task<List<EventDTO>> GetEventsByTypeDTO(string eventType);
    }
}