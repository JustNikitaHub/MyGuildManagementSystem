using GuildManagement.Entities;

namespace GuildManagement.Interfaces
{
    public interface IMemberRepository : IRepository<Member>
    {
        Task<IEnumerable<Member>> GetByLevelRange(int minLevel, int maxLevel);
        Task<IEnumerable<Member>> GetByClass(MemberClass memberClass);
        Task<Member?> GetByName(string name);
        Task<bool> ChangeLevel(int memberId, int newLevel);
        Task<Member> UpdateLastActive(int memberId);
    }
}