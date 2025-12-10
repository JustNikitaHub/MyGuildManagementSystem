using GuildManagement.Entities;

namespace GuildManagement.Interfaces
{
    public interface IResourceRepository : IRepository<Resource>
    {
        Task<IEnumerable<Resource>> GetByMemberId(int memberId);
        Task<IEnumerable<Resource>> GetByType(ResourceType type);
        Task<IEnumerable<Resource>> GetByRarity(Rarity rarity);
        Task<bool> UpdateQuantity(int resourceId, int newQuantity);
    }
}