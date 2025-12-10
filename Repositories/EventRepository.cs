using Microsoft.EntityFrameworkCore;
using GuildManagement.Data;
using GuildManagement.Entities;
using GuildManagement.Interfaces;
using System.Linq.Expressions;

namespace GuildManagement.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly GuildManagementContext _context;

        public EventRepository(GuildManagementContext context)
        {
            _context = context;
        }

        public async Task<Event?> GetById(int id)
        {
            return await _context.Events
                .Include(e => e.Member)
                .Include(e => e.Achievement)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Event>> GetAll()
        {
            return await _context.Events
                .Include(e => e.Member)
                .Include(e => e.Achievement)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> Find(Expression<Func<Event, bool>> predicate)
        {
            return await _context.Events
                .Where(predicate)
                .Include(e => e.Member)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetByTitle(string title)
        {
            return await _context.Events
                .Where(e => e.Title.Contains(title))
                .Include(e => e.Member)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetByMemberId(int memberId)
        {
            return await _context.Events
                .Where(e => e.MemberId == memberId)
                .Include(e => e.Member)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetByType(EventType type)
        {
            return await _context.Events
                .Where(e => e.Type == type)
                .Include(e => e.Member)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingEvents()
        {
            return await _context.Events
                .Where(e => e.StartDate >= DateTime.Now)
                .OrderBy(e => e.StartDate)
                .Include(e => e.Member)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetByDateRange(DateTime start, DateTime end)
        {
            return await _context.Events
                .Where(e => e.StartDate >= start && e.StartDate <= end)
                .Include(e => e.Member)
                .ToListAsync();
        }

        public async Task<Event> Add(Event entity)
        {
            _context.Events.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Event> Update(Event entity)
        {
            _context.Events.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task Delete(int id)
        {
            var entity = await GetById(id);
            if (entity != null)
            {
                _context.Events.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Exists(int id)
        {
            return await _context.Events.AnyAsync(e => e.Id == id);
        }

        public async Task<bool> CompleteEvent(int eventId, int? achievementId = null)
        {
            var eventEntity = await GetById(eventId);
            if (eventEntity == null) return false;

            
            eventEntity.EndDate = DateTime.Now;
            
            if (achievementId.HasValue)
            {
                var achievement = await _context.Achievements.FindAsync(achievementId);
                eventEntity.Achievement = achievement;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}