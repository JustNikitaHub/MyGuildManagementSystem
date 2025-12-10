using Microsoft.EntityFrameworkCore;
using GuildManagement.Data;
using GuildManagement.Entities;
using GuildManagement.Interfaces;
using System.Linq.Expressions;

namespace GuildManagement.Repositories
{
    public class AchievementRepository : IAchievementRepository
    {
        private readonly GuildManagementContext _context;

        public AchievementRepository(GuildManagementContext context)
        {
            _context = context;
        }

        public async Task<Achievement?> GetById(int id)
        {
            return await _context.Achievements
                .Include(a => a.Member)
                .Include(a => a.Event)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Achievement>> GetAll()
        {
            return await _context.Achievements
                .Include(a => a.Member)
                .Include(a => a.Event)
                .ToListAsync();
        }

        public async Task<IEnumerable<Achievement>> Find(Expression<Func<Achievement, bool>> predicate)
        {
            return await _context.Achievements
                .Where(predicate)
                .Include(a => a.Member)
                .ToListAsync();
        }

        public async Task<IEnumerable<Achievement>> GetByMemberId(int memberId)
        {
            return await _context.Achievements
                .Where(a => a.MemberId == memberId)
                .Include(a => a.Event)
                .OrderByDescending(a => a.EarnedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Achievement>> GetByEventId(int eventId)
        {
            return await _context.Achievements
                .Where(a => a.EventId == eventId)
                .Include(a => a.Member)
                .ToListAsync();
        }

        public async Task<IEnumerable<Achievement>> GetRecentAchievements(int days)
        {
            var dateThreshold = DateTime.Now.AddDays(-days);
            return await _context.Achievements
                .Where(a => a.EarnedDate >= dateThreshold)
                .Include(a => a.Member)
                .Include(a => a.Event)
                .OrderByDescending(a => a.EarnedDate)
                .ToListAsync();
        }

        public async Task<Achievement> Add(Achievement entity)
        {
            entity.EarnedDate = DateTime.Now;
            _context.Achievements.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Achievement> Update(Achievement entity)
        {
            _context.Achievements.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task Delete(int id)
        {
            var entity = await GetById(id);
            if (entity != null)
            {
                _context.Achievements.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Exists(int id)
        {
            return await _context.Achievements.AnyAsync(a => a.Id == id);
        }
    }
}