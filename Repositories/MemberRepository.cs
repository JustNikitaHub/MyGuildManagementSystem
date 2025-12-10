using Microsoft.EntityFrameworkCore;
using GuildManagement.Data;
using GuildManagement.Entities;
using GuildManagement.Interfaces;
using System.Linq.Expressions;

namespace GuildManagement.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly GuildManagementContext _context;

        public MemberRepository(GuildManagementContext context)
        {
            _context = context;
        }

        public async Task<Member?> GetById(int id)
        {
            return await _context.Members
                .Include(m => m.Events)
                .Include(m => m.Achievements)
                .Include(m => m.Resources)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Member>> GetAll()
        {
            return await _context.Members
                .Include(m => m.Events)
                .Include(m => m.Achievements)
                .Include(m => m.Resources)
                .ToListAsync();
        }

        public async Task<IEnumerable<Member>> Find(Expression<Func<Member, bool>> predicate)
        {
            return await _context.Members
                .Where(predicate)
                .Include(m => m.Events)
                .Include(m => m.Achievements)
                .ToListAsync();
        }

        public async Task<IEnumerable<Member>> GetByLevelRange(int minLevel, int maxLevel)
        {
            return await _context.Members
                .Where(m => m.Level >= minLevel && m.Level <= maxLevel)
                .Include(m => m.Events)
                .ToListAsync();
        }

        public async Task<IEnumerable<Member>> GetByClass(MemberClass memberClass)
        {
            return await _context.Members
                .Where(m => m.MemberClass == memberClass)
                .Include(m => m.Achievements)
                .ToListAsync();
        }

        public async Task<Member?> GetByName(string name)
        {
            return await _context.Members
                .FirstOrDefaultAsync(m => m.Name == name);
        }

        public async Task<Member> Add(Member member)
        {
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<Member> Update(Member member)
        {
            _context.Members.Update(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task Delete(int id)
        {
            var member = await GetById(id);
            if (member != null)
            {
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Exists(int id)
        {
            return await _context.Members.AnyAsync(m => m.Id == id);
        }

        public async Task<bool> ChangeLevel(int memberId, int newLevel)
        {
            var member = await GetById(memberId);
            if (member == null) return false;

            member.Level = newLevel;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Member> UpdateLastActive(int memberId)
        {
            var member = await GetById(memberId);
            if (member != null)
            {
                await _context.SaveChangesAsync();
            }
            return member;
        }
    }
}