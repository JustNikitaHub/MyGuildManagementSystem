using GuildManagement.Entities;

namespace GuildManagement.Interfaces
{
    public interface IMemberService
    {
        Task<List<Member>> GetAllMembers();
        Task<Member?> GetMemberById(int id);
        Task<Member> CreateMember(Member Member);
        Task<Member> UpdateMember(int id, Member Member);
        Task<bool> DeleteMember(int id);
        Task<List<Member>> GetMembersByLevel(int minLevel, int maxLevel);
        Task<List<Member>> GetMembersByClass(MemberClass memberClass);
        Task<bool> ChangeMemberLevel(int memberId, int newLevel);
        Task<Member> UpdateLastActive(int memberId);
    }
}