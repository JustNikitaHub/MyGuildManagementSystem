using GuildManagement.Entities;

namespace GuildManagement.Interfaces
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<IEnumerable<Event>> GetByTitle(string title);
        Task<IEnumerable<Event>> GetByMemberId(int memberId);
        Task<IEnumerable<Event>> GetByType(EventType type);
        Task<IEnumerable<Event>> GetUpcomingEvents();
        Task<IEnumerable<Event>> GetByDateRange(DateTime start, DateTime end);
        Task<bool> CompleteEvent(int eventId, int? achievementId = null);
    }
}