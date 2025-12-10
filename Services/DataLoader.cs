using GuildManagement.Data;
using GuildManagement.Entities;

namespace GuildManagement.Services
{
    public class DataLoader
    {
        private readonly GuildManagementContext _context;

        public DataLoader(GuildManagementContext context)
        {
            _context = context;
        }

        public async Task LoadTestData()
        {
            //чистсим базу
            _context.Members.RemoveRange(_context.Members);
            _context.Events.RemoveRange(_context.Events);
            _context.Achievements.RemoveRange(_context.Achievements);
            _context.Resources.RemoveRange(_context.Resources);
            await _context.SaveChangesAsync();

            //тетстовые данные
            var member1 = new Member("Артас Менетил", 60, MemberClass.Warrior);
            var member2 = new Member("Джайна Праудмур", 58, MemberClass.Mage);
            var member3 = new Member("Валера Сангуинар", 57, MemberClass.Rogue);

            _context.Members.Add(member1);
            _context.Members.Add(member2);
            _context.Members.Add(member3);
            await _context.SaveChangesAsync();

            var event1 = new Event("Рейд на Наксрамас", EventType.Raid, DateTime.Now.AddDays(1), member1.Id);
            var event2 = new Event("Ежедневные задания", EventType.Dungeon, DateTime.Now, member2.Id);
            var event3 = new Event("Гильдейская встреча", EventType.Social, DateTime.Now.AddDays(2), member3.Id);

            _context.Events.Add(event1);
            _context.Events.Add(event2);
            _context.Events.Add(event3);
            await _context.SaveChangesAsync();

            //какието события
            var achievement1 = new Achievement("Покоритель Наксрамаса", "Пройден рейд Наксрамас", member1.Id);
            var achievement2 = new Achievement("Мастер заданий", "Выполнено 100 ежедневных заданий", member2.Id);

            _context.Achievements.Add(achievement1);
            _context.Achievements.Add(achievement2);
            await _context.SaveChangesAsync();

            var resource1 = new Resource("Золотой самородок", ResourceType.Gold, 150, member1.Id) 
            { 
                Rarity = Rarity.Rare,
                Description = "Ценный золотой самородок"
            };
            
            var resource2 = new Resource("Мифриловая руда", ResourceType.Iron, 500, member2.Id)
            {
                Rarity = Rarity.Uncommon
            };

            _context.Resources.Add(resource1);
            _context.Resources.Add(resource2);
            await _context.SaveChangesAsync();

        }
    }
}