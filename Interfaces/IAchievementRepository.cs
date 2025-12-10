using GuildManagement.Entities;

namespace GuildManagement.Interfaces
{
    public interface IAchievementRepository : IRepository<Achievement>
    {
        Task<IEnumerable<Achievement>> GetByMemberId(int memberId);
        Task<IEnumerable<Achievement>> GetByEventId(int eventId);
        Task<IEnumerable<Achievement>> GetRecentAchievements(int days);
    }
}