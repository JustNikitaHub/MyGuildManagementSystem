using Xunit;
using Moq;
using GuildManagement.Services;
using GuildManagement.Interfaces;
using GuildManagement.Entities;
using GuildManagement.DTOs;
using Microsoft.EntityFrameworkCore;
using GuildManagement.Data;

namespace GuildManagement.Tests.Services
{
    public class EventServiceTests : IDisposable
    {
        private readonly Mock<IEventRepository> _eventRepositoryMock;
        private readonly Mock<IAchievementRepository> _achievementRepositoryMock;
        private readonly GuildManagementContext _context;
        private readonly EventService _eventService;
        private readonly Member _testMember;

        public EventServiceTests()
        {
            _eventRepositoryMock = new Mock<IEventRepository>();
            _achievementRepositoryMock = new Mock<IAchievementRepository>();
            
            var options = new DbContextOptionsBuilder<GuildManagementContext>()
                .UseInMemoryDatabase(databaseName: $"EventTestDb_{Guid.NewGuid()}")
                .Options;
            
            _context = new GuildManagementContext(options);
        
            _testMember = new Member("Тестовый участник", 50, MemberClass.Warrior) { Id = 1 };
            _context.Members.Add(_testMember);
            _context.SaveChanges();
            
            _eventService = new EventService(
                _eventRepositoryMock.Object, 
                _achievementRepositoryMock.Object, 
                _context);
            
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllEvents_ShouldReturnAllEvents()
        {
            var expectedEvents = new List<Event>
            {
                new Event("Рейд на Наксрамас", EventType.Raid, DateTime.Now.AddDays(1), 1) { Id = 1 },
                new Event("Ежедневные задания", EventType.Dungeon, DateTime.Now, 2) { Id = 2 }
            };
            _eventRepositoryMock
                .Setup(repo => repo.GetAll())
                .ReturnsAsync(expectedEvents);
            var result = await _eventService.GetAllEvents();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Рейд на Наксрамас", result[0].Title);
            _eventRepositoryMock.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetEventById_WhenEventExists_ShouldReturnEvent()
        {
            var expectedEvent = new Event("Рейд на Наксрамас", EventType.Raid, DateTime.Now.AddDays(1), 1) 
            { 
                Id = 1,
                Description = "Описание рейда"
            };
            _eventRepositoryMock
                .Setup(repo => repo.GetById(1))
                .ReturnsAsync(expectedEvent);
            var result = await _eventService.GetEventById(1);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Рейд на Наксрамас", result.Title);
            Assert.Equal(EventType.Raid, result.Type);
            _eventRepositoryMock.Verify(repo => repo.GetById(1), Times.Once);
        }

        [Fact]
        public async Task CreateEvent_WithFutureDate_ShouldReturnCreatedEvent()
        {
            var newEvent = new Event("Новое событие", EventType.Social, DateTime.Now.AddDays(1), 1);
            var createdEvent = new Event("Новое событие", EventType.Social, DateTime.Now.AddDays(1), 1) 
            { 
                Id = 100,
                EndDate = newEvent.StartDate.AddHours(2)
            };
            
            _eventRepositoryMock
                .Setup(repo => repo.Add(It.IsAny<Event>()))
                .ReturnsAsync(createdEvent);
            var result = await _eventService.CreateEvent(newEvent);
            Assert.NotNull(result);
            Assert.Equal(100, result.Id);
            Assert.Equal("Новое событие", result.Title);
            _eventRepositoryMock.Verify(repo => repo.Add(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public async Task CreateEvent_WithPastDate_ShouldThrowArgumentException()
        {
            var pastEvent = new Event("Прошедшее событие", EventType.Raid, DateTime.Now.AddDays(-1), 1);
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _eventService.CreateEvent(pastEvent));
        }

        [Fact]
        public async Task UpdateEvent_WhenEventExists_ShouldUpdateAndReturnEvent()
        {
            // Arrange
            var existingEvent = new Event("Старое название", EventType.Raid, DateTime.Now.AddDays(1), 1) 
            { 
                Id = 1,
                Description = "Старое описание"
            };
            
            var updatedEventData = new Event("Новое название", EventType.Dungeon, DateTime.Now.AddDays(2), 2)
            {
                Description = "Новое описание"
            };
            
            _eventRepositoryMock
                .Setup(repo => repo.GetById(1))
                .ReturnsAsync(existingEvent);
            
            _eventRepositoryMock
                .Setup(repo => repo.Update(It.Is<Event>(e => e.Id == 1)))
                .ReturnsAsync(existingEvent);

            // Act
            var result = await _eventService.UpdateEvent(1, updatedEventData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Новое название", result.Title);
            Assert.Equal(EventType.Dungeon, result.Type);
            Assert.Equal("Новое описание", result.Description);
            _eventRepositoryMock.Verify(repo => repo.GetById(1), Times.Once);
            _eventRepositoryMock.Verify(repo => repo.Update(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public async Task DeleteEvent_ShouldReturnTrueOnSuccess()
        {
            // Arrange
            _eventRepositoryMock
                .Setup(repo => repo.Delete(1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _eventService.DeleteEvent(1);

            // Assert
            Assert.True(result);
            _eventRepositoryMock.Verify(repo => repo.Delete(1), Times.Once);
        }

        [Fact]
        public async Task GetEventsByMemberId_ShouldReturnMemberEvents()
        {
            // Arrange
            var memberEvents = new List<Event>
            {
                new Event("Рейд 1", EventType.Raid, DateTime.Now.AddDays(1), 1) { Id = 1 },
                new Event("Рейд 2", EventType.Raid, DateTime.Now.AddDays(2), 1) { Id = 2 }
            };
            
            _eventRepositoryMock
                .Setup(repo => repo.GetByMemberId(1))
                .ReturnsAsync(memberEvents);

            // Act
            var result = await _eventService.GetEventsByMemberId(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.Equal(1, e.MemberId));
            _eventRepositoryMock.Verify(repo => repo.GetByMemberId(1), Times.Once);
        }

        [Fact]
        public async Task GetUpcomingEvents_ShouldReturnFutureEvents()
        {
            // Arrange
            var upcomingEvents = new List<Event>
            {
                new Event("Будущий рейд", EventType.Raid, DateTime.Now.AddDays(2), 1) { Id = 1 },
                new Event("Завтрашняя встреча", EventType.Social, DateTime.Now.AddDays(1), 2) { Id = 2 }
            };
            
            _eventRepositoryMock
                .Setup(repo => repo.GetUpcomingEvents())
                .ReturnsAsync(upcomingEvents);

            // Act
            var result = await _eventService.GetUpcomingEvents();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.True(e.StartDate > DateTime.Now));
            _eventRepositoryMock.Verify(repo => repo.GetUpcomingEvents(), Times.Once);
        }

        [Fact]
        public async Task GetEventsByDateRange_WithValidRange_ShouldReturnEvents()
        {
            // Arrange
            var startDate = DateTime.Now.AddDays(1);
            var endDate = DateTime.Now.AddDays(7);
            var events = new List<Event>
            {
                new Event("Событие в диапазоне", EventType.Raid, DateTime.Now.AddDays(3), 1) { Id = 1 }
            };
            
            _eventRepositoryMock
                .Setup(repo => repo.GetByDateRange(startDate, endDate))
                .ReturnsAsync(events);

            // Act
            var result = await _eventService.GetEventsByDateRange(startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _eventRepositoryMock.Verify(repo => repo.GetByDateRange(startDate, endDate), Times.Once);
        }

        [Fact]
        public async Task GetEventsByDateRange_WithInvalidRange_ShouldThrowArgumentException()
        {
            // Arrange
            var startDate = DateTime.Now.AddDays(7);
            var endDate = DateTime.Now.AddDays(1);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _eventService.GetEventsByDateRange(startDate, endDate));
        }

        [Fact]
        public async Task CompleteEvent_WithAchievement_ShouldReturnTrue()
        {
            // Arrange
            var achievement = new Achievement("Достижение рейда", "Пройден рейд", 1);
            var createdAchievement = new Achievement("Достижение рейда", "Пройден рейд", 1) { Id = 50 };
            
            _achievementRepositoryMock
                .Setup(repo => repo.Add(It.IsAny<Achievement>()))
                .ReturnsAsync(createdAchievement);
            
            _eventRepositoryMock
                .Setup(repo => repo.CompleteEvent(1, 50))
                .ReturnsAsync(true);

            // Act
            var result = await _eventService.CompleteEvent(1, achievement);

            // Assert
            Assert.True(result);
            _achievementRepositoryMock.Verify(repo => repo.Add(It.IsAny<Achievement>()), Times.Once);
            _eventRepositoryMock.Verify(repo => repo.CompleteEvent(1, 50), Times.Once);
        }

        [Fact]
        public async Task CompleteEvent_WithoutAchievement_ShouldReturnTrue()
        {
            // Arrange
            _eventRepositoryMock
                .Setup(repo => repo.CompleteEvent(1, null))
                .ReturnsAsync(true);

            // Act
            var result = await _eventService.CompleteEvent(1, null);

            // Assert
            Assert.True(result);
            _eventRepositoryMock.Verify(repo => repo.CompleteEvent(1, null), Times.Once);
        }

        [Fact]
        public async Task GetAllEventsDTO_ShouldReturnEventDTOs()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event("Тестовый рейд", EventType.Raid, DateTime.Now.AddDays(1), _testMember.Id) 
                { 
                    Id = 1,
                    Description = "Описание рейда",
                    Member = _testMember
                }
            };

            await _context.Events.AddRangeAsync(events);
            await _context.SaveChangesAsync();

            // Act
            var result = await _eventService.GetAllEventsDTO();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            
            var eventDto = result.First();
            Assert.Equal("Тестовый рейд", eventDto.Title);
            Assert.Equal("Raid", eventDto.Type);
            Assert.Equal(1, eventDto.MemberId);
            Assert.Equal("Тестовый участник", eventDto.MemberName);
            Assert.True(eventDto.IsUpcoming);
        }

        [Fact]
        public async Task CreateEventDTO_WithValidData_ShouldReturnEventDTO()
        {
            // Arrange
            var createEventDto = new CreateEventDTO
            {
                Title = "Новый DTO ивент",
                Description = "Описание DTO ивента",
                Type = "Raid",
                StartDate = DateTime.Now.AddDays(1),
                MemberId = _testMember.Id
            };

            var createdEvent = new Event("Новый DTO ивент", EventType.Raid, DateTime.Now.AddDays(1), _testMember.Id) 
            { 
                Id = 100,
                Description = "Описание DTO ивента"
            };
            
            _eventRepositoryMock
                .Setup(repo => repo.Add(It.IsAny<Event>()))
                .ReturnsAsync(createdEvent);

            // Act
            var result = await _eventService.CreateEventDTO(createEventDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Id);
            Assert.Equal("Новый DTO ивент", result.Title);
            Assert.Equal("Raid", result.Type);
            _eventRepositoryMock.Verify(repo => repo.Add(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public async Task CreateEventDTO_WithInvalidMember_ShouldThrowArgumentException()
        {
            // Arrange
            var createEventDto = new CreateEventDTO
            {
                Title = "Новый ивент",
                Type = "Raid",
                StartDate = DateTime.Now.AddDays(1),
                MemberId = 999 // Несуществующий участник
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _eventService.CreateEventDTO(createEventDto));
        }

        [Fact]
        public async Task CreateEventDTO_WithInvalidType_ShouldThrowArgumentException()
        {
            // Arrange
            var createEventDto = new CreateEventDTO
            {
                Title = "Новый ивент",
                Type = "InvalidType",
                StartDate = DateTime.Now.AddDays(1),
                MemberId = _testMember.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _eventService.CreateEventDTO(createEventDto));
        }
    }
}