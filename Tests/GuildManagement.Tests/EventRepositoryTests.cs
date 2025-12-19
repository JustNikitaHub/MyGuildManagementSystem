using Xunit;
using Microsoft.EntityFrameworkCore;
using GuildManagement.Repositories;
using GuildManagement.Entities;
using GuildManagement.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace GuildManagement.Tests.Repositories
{
    public class EventRepositoryTests : IDisposable
    {
        private readonly GuildManagementContext _context;
        private readonly EventRepository _repository;
        private readonly Member _testMember;

        public EventRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<GuildManagementContext>()
                .UseInMemoryDatabase(databaseName: $"EventRepoTestDb_{Guid.NewGuid()}")
                .Options;
            
            _context = new GuildManagementContext(options);
            _repository = new EventRepository(_context);
            
            _testMember = new Member("Тестовый участник", 50, MemberClass.Warrior);
            _context.Members.Add(_testMember);
            
            SeedTestData();
            _context.SaveChanges();
        }

        private void SeedTestData()
        {
            var events = new List<Event>
            {
                new Event("Рейд на Наксрамас", EventType.Raid, DateTime.Now.AddDays(1), _testMember.Id),
                new Event("Ежедневные задания", EventType.Dungeon, DateTime.Now, _testMember.Id),
                new Event("Гильдейская встреча", EventType.Social, DateTime.Now.AddDays(2), _testMember.Id),
                new Event("Прошедший рейд", EventType.Raid, DateTime.Now.AddDays(-1), _testMember.Id),
                new Event("Будущий дуэль", EventType.Dungeon, DateTime.Now.AddDays(3), _testMember.Id)
            };

            _context.Events.AddRange(events);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllEvents()
        {
            var result = await _repository.GetAll();
            Assert.NotNull(result);
            Assert.Equal(5, result.Count());
        }

        [Fact]
        public async Task GetById_WhenEventExists_ShouldReturnEventWithMember()
        {
            var eventEntity = _context.Events.First();
            var result = await _repository.GetById(eventEntity.Id);
            Assert.NotNull(result);
            Assert.NotNull(result.Member);
            Assert.Equal(eventEntity.Id, result.Id);
            Assert.Equal(eventEntity.Title, result.Title);
        }

        [Fact]
        public async Task Add_ShouldAddNewEvent()
        {
            var newEvent = new Event("Новое событие", EventType.Social, DateTime.Now.AddDays(5), _testMember.Id);
            var result = await _repository.Add(newEvent);
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Новое событие", result.Title);
            
            var dbEvent = await _context.Events.FindAsync(result.Id);
            Assert.NotNull(dbEvent);
        }

        [Fact]
        public async Task GetUpcomingEvents_ShouldReturnFutureEvents()
        {

            var result = await _repository.GetUpcomingEvents();

            Assert.NotNull(result);
            Assert.All(result, e => Assert.True(e.StartDate >= DateTime.Now));
        }

        [Fact]
        public async Task GetByMemberId_ShouldReturnMemberEvents()
        {
            var result = await _repository.GetByMemberId(_testMember.Id);
            Assert.NotNull(result);
            Assert.All(result, e => Assert.Equal(_testMember.Id, e.MemberId));
        }

        [Fact]
        public async Task GetByType_ShouldReturnFilteredEvents()
        {
            var result = await _repository.GetByType(EventType.Raid);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, e => Assert.Equal(EventType.Raid, e.Type));
        }

        [Fact]
        public async Task GetByDateRange_ShouldReturnEventsInRange()
        {
            var startDate = DateTime.Now.AddDays(-2);
            var endDate = DateTime.Now.AddDays(2);
            var result = await _repository.GetByDateRange(startDate, endDate);
            Assert.NotNull(result);
            Assert.All(result, e => 
            {
                Assert.True(e.StartDate >= startDate);
                Assert.True(e.StartDate <= endDate);
            });
        }

        [Fact]
        public async Task CompleteEvent_ShouldUpdateEndDate()
        {

            var eventEntity = _context.Events.First(e => e.StartDate > DateTime.Now);


            var result = await _repository.CompleteEvent(eventEntity.Id);

         
            Assert.True(result);
            
            var completedEvent = await _context.Events.FindAsync(eventEntity.Id);
            Assert.NotNull(completedEvent);
            Assert.True(completedEvent.EndDate <= DateTime.Now);
        }

        [Fact]
        public async Task CompleteEvent_WithAchievementId_ShouldSetAchievement()
        {
       
            var eventEntity = _context.Events.First(e => e.StartDate > DateTime.Now);
            var achievement = new Achievement("Тестовое достижение", "Описание", _testMember.Id);
            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();

  
            var result = await _repository.CompleteEvent(eventEntity.Id, achievement.Id);

      
            Assert.True(result);
            
            var completedEvent = await _context.Events
                .Include(e => e.Achievement)
                .FirstOrDefaultAsync(e => e.Id == eventEntity.Id);
            
            Assert.NotNull(completedEvent);
            Assert.NotNull(completedEvent.Achievement);
            Assert.Equal(achievement.Id, completedEvent.Achievement.Id);
        }
    }
}