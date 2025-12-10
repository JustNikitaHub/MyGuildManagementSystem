using GuildManagement.Entities;
using GuildManagement.Interfaces;

namespace GuildManagement.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepository;

        public MemberService(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        public async Task<List<Member>> GetAllMembers()
        {
            return (await _memberRepository.GetAll()).ToList();
        }

        public async Task<Member?> GetMemberById(int id)
        {
            return await _memberRepository.GetById(id);
        }

        public async Task<Member> CreateMember(Member member)
        {
            if (member.Level < 1 || member.Level > 60)
            {
                throw new ArgumentException("Уровень должен быть от 1 до 60");
            }
            if (string.IsNullOrWhiteSpace(member.Name))
            {
                throw new ArgumentException("Имя не может быть пустым");
            }

            return await _memberRepository.Add(member);
        }

        public async Task<Member> UpdateMember(int id, Member member)
        {
            var existingMember = await _memberRepository.GetById(id);
            if (existingMember == null)
            {
                throw new ArgumentException("Участник не найден");
            }
            existingMember.Name = member.Name;
            existingMember.Level = member.Level;
            existingMember.MemberClass = member.MemberClass;

            return await _memberRepository.Update(existingMember);
        }

        public async Task<bool> DeleteMember(int id)
        {
            try
            {
                await _memberRepository.Delete(id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Member>> GetMembersByLevel(int minLevel, int maxLevel)
        {
            if (minLevel > maxLevel)
            {
                throw new ArgumentException("Минимальный уровень не может быть больше максимального");
            }

            return (await _memberRepository.GetByLevelRange(minLevel, maxLevel)).ToList();
        }

        public async Task<List<Member>> GetMembersByClass(MemberClass memberClass)
        {
            return (await _memberRepository.GetByClass(memberClass)).ToList();
        }

        public async Task<bool> ChangeMemberLevel(int memberId, int newLevel)
        {
            if (newLevel < 1 || newLevel > 60)
            {
                throw new ArgumentException("Уровень должен быть от 1 до 60");
            }

            return await _memberRepository.ChangeLevel(memberId, newLevel);
        }

        public async Task<Member> UpdateLastActive(int memberId)
        {
            return await _memberRepository.UpdateLastActive(memberId);
        }
    }
}