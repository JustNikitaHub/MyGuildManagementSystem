using GuildManagement.DTOs;
using GuildManagement.Entities;
using GuildManagement.Interfaces;
using Microsoft.EntityFrameworkCore;
using GuildManagement.Data;

namespace GuildManagement.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly GuildManagementContext _context;

        public MemberService(IMemberRepository memberRepository, GuildManagementContext context)
        {
            _memberRepository = memberRepository;
            _context = context;
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

        //новое
        public async Task<List<MemberDTO>> GetAllMembersDTO()
        {
            var members = await _context.Members
                .Include(m => m.Events)
                .Include(m => m.Achievements)
                .Include(m => m.Resources)
                .ToListAsync();

            return members.Select(ConvertToDTO).ToList();
        }

        public async Task<MemberDTO?> GetMemberDTOById(int id)
        {
            var member = await _context.Members
                .Include(m => m.Events)
                .Include(m => m.Achievements)
                .Include(m => m.Resources)
                .FirstOrDefaultAsync(m => m.Id == id);

            return member != null ? ConvertToDTO(member) : null;
        }

        public async Task<MemberDTO> CreateMemberDTO(CreateMemberDTO memberDto)
        {
            if (!Enum.TryParse<MemberClass>(memberDto.MemberClass, out var memberClass))
            {
                throw new ArgumentException("Неверный класс участника");
            }

            var member = new Member
            {
                Name = memberDto.Name,
                Level = memberDto.Level,
                MemberClass = memberClass
            };
            var createdMember = await _memberRepository.Add(member);

            return ConvertToDTO(createdMember);
        }

        public async Task<MemberDTO> UpdateMemberDTO(int id, CreateMemberDTO memberDto)
        {
            var existingMember = await _memberRepository.GetById(id);
            if (existingMember == null)
            {
                throw new ArgumentException("Участник не найден");
            }

            if (!Enum.TryParse<MemberClass>(memberDto.MemberClass, out var memberClass))
            {
                throw new ArgumentException("Неверный класс участника");
            }

            existingMember.Name = memberDto.Name;
            existingMember.Level = memberDto.Level;
            existingMember.MemberClass = memberClass;
            var updatedMember = await _memberRepository.Update(existingMember);

            return ConvertToDTO(updatedMember);
        }
        public async Task<List<MemberDTO>> GetMembersByLevelDTO(int minLevel, int maxLevel)
        {
            if (minLevel > maxLevel)
            {
                throw new ArgumentException("Минимальный уровень не может быть больше максимального");
            }

            var members = await _context.Members
                .Where(m => m.Level >= minLevel && m.Level <= maxLevel)
                .Include(m => m.Events)
                .ToListAsync();

            return members.Select(ConvertToDTO).ToList();
        }

        public async Task<List<MemberDTO>> GetMembersByClassDTO(string memberClass)
        {
            if (!Enum.TryParse<MemberClass>(memberClass, out var memberClassEnum))
            {
                throw new ArgumentException("Неверный класс участника");
            }

            var members = await _context.Members
                .Where(m => m.MemberClass == memberClassEnum)
                .Include(m => m.Achievements)
                .ToListAsync();

            return members.Select(ConvertToDTO).ToList();
        }
        private MemberDTO ConvertToDTO(Member member)
        {
            return new MemberDTO
            {
                Id = member.Id,
                Name = member.Name,
                Level = member.Level,
                MemberClass = member.MemberClass.ToString(),
                EventIds = member.Events?.Select(e => e.Id).ToList() ?? new(),
                AchievementIds = member.Achievements?.Select(a => a.Id).ToList() ?? new(),
                ResourceIds = member.Resources?.Select(r => r.Id).ToList() ?? new(),
                EventsCount = member.Events?.Count ?? 0,
                AchievementsCount = member.Achievements?.Count ?? 0,
                ResourcesCount = member.Resources?.Count ?? 0
            };
        }
    }
}