using Microsoft.EntityFrameworkCore;
using GuildManagement.Data;
using GuildManagement.Entities;
using GuildManagement.Interfaces;
using System.Linq.Expressions;

namespace GuildManagement.Repositories
{
    public class ResourceRepository : IResourceRepository
    {
        private readonly GuildManagementContext _context;

        public ResourceRepository(GuildManagementContext context)
        {
            _context = context;
        }

        public async Task<Resource?> GetById(int id)
        {
            return await _context.Resources
                .Include(r => r.Member)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Resource>> GetAll()
        {
            return await _context.Resources
                .Include(r => r.Member)
                .ToListAsync();
        }

        public async Task<IEnumerable<Resource>> Find(Expression<Func<Resource, bool>> predicate)
        {
            return await _context.Resources
                .Where(predicate)
                .Include(r => r.Member)
                .ToListAsync();
        }

        public async Task<IEnumerable<Resource>> GetByMemberId(int memberId)
        {
            return await _context.Resources
                .Where(r => r.MemberId == memberId)
                .Include(r => r.Member)
                .ToListAsync();
        }

        public async Task<IEnumerable<Resource>> GetByType(ResourceType type)
        {
            return await _context.Resources
                .Where(r => r.Type == type)
                .Include(r => r.Member)
                .ToListAsync();
        }

        public async Task<IEnumerable<Resource>> GetByRarity(Rarity rarity)
        {
            return await _context.Resources
                .Where(r => r.Rarity == rarity)
                .Include(r => r.Member)
                .ToListAsync();
        }

        public async Task<Resource> Add(Resource entity)
        {
            _context.Resources.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Resource> Update(Resource entity)
        {
            _context.Resources.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task Delete(int id)
        {
            var entity = await GetById(id);
            if (entity != null)
            {
                _context.Resources.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Exists(int id)
        {
            return await _context.Resources.AnyAsync(r => r.Id == id);
        }

        public async Task<bool> UpdateQuantity(int resourceId, int newQuantity)
        {
            var resource = await GetById(resourceId);
            if (resource == null) return false;

            resource.Quantity = newQuantity;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}