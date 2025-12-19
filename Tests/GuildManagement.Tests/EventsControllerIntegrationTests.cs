using Xunit;
using Microsoft.AspNetCore.Mvc;
using GuildManagement.Controllers;
using GuildManagement.Data;
using GuildManagement.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using GuildManagement.Entities;
using System.Linq;
using System;

namespace GuildManagement.Tests.Controllers
{
    public class EventsControllerSimpleIntegrationTests : IDisposable
    {
        private readonly GuildManagementContext _context;
        private readonly EventsController _controller;

        public EventsControllerSimpleIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<GuildManagementContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            
            _context = new GuildManagementContext(options);
            _context.Database.EnsureCreated();
            
            _controller = new EventsController(_context);
        }

        private async Task<Member> CreateTestMember(string name = "Тестовый участник")
        {
            var member = new Member(name, 50, MemberClass.Warrior);
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        private async Task<Event> CreateTestEvent(Member member, string title = "Тестовое событие")
        {
            var eventEntity = new Event(title, EventType.Raid, DateTime.Now.AddDays(1), member.Id)
            {
                Member = member
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();
            return eventEntity;
        }

        [Fact]
        public async Task GetAllEvents_ReturnsOkResult_WithEventsList()
        {
            var member = await CreateTestMember();
            await CreateTestEvent(member, "Рейд на Наксрамас");
            await CreateTestEvent(member, "Гильдейская встреча");
            var result = await _controller.GetEvents();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var events = Assert.IsType<List<EventDTO>>(okResult.Value);
            Assert.Equal(2, events.Count);
        }

        [Fact]
        public async Task GetEventById_WhenEventExists_ReturnsEvent()
        {

            var member = await CreateTestMember();
            var eventEntity = await CreateTestEvent(member, "Рейд на Наксрамас");


            var result = await _controller.GetEvent(eventEntity.Id);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var eventDto = Assert.IsType<EventDTO>(okResult.Value);
            Assert.Equal(eventEntity.Id, eventDto.Id);
            Assert.Equal("Рейд на Наксрамас", eventDto.Title);
            Assert.Equal("Raid", eventDto.Type);
            Assert.Equal(member.Id, eventDto.MemberId);
        }

        [Fact]
        public async Task CreateEvent_WithValidData_ReturnsCreatedEvent()
        {
            var member = await CreateTestMember();
            
            var newEvent = new CreateEventDTO
            {
                Title = "Новый рейд",
                Description = "Описание нового рейда",
                Type = "Raid",
                StartDate = DateTime.Now.AddDays(1),
                MemberId = member.Id
            };
            var result = await _controller.PostEvent(newEvent);
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var eventDto = Assert.IsType<EventDTO>(createdAtResult.Value);
            Assert.Equal("Новый рейд", eventDto.Title);
            Assert.Equal("Raid", eventDto.Type);
            Assert.Equal(member.Id, eventDto.MemberId);
            var dbEvent = await _context.Events.FindAsync(eventDto.Id);
            Assert.NotNull(dbEvent);
            Assert.Equal("Новый рейд", dbEvent.Title);
        }

        [Fact]
        public async Task GetUpcomingEvents_ReturnsOnlyFutureEvents()
        {
            var member = await CreateTestMember();
            var pastEvent = new Event("Прошедший рейд", EventType.Raid, DateTime.Now.AddDays(-1), member.Id);
            _context.Events.Add(pastEvent);
            await CreateTestEvent(member, "Будущий рейд 1");
            await CreateTestEvent(member, "Будущий рейд 2");
            await _context.SaveChangesAsync();
            var result = await _controller.GetUpcomingEvents();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var events = Assert.IsType<List<EventDTO>>(okResult.Value);
            Assert.Equal(2, events.Count);
            Assert.All(events, e => Assert.True(e.IsUpcoming));
        }

        [Fact]
        public async Task GetEventsByMember_ReturnsMemberEvents()
        {
            var member1 = await CreateTestMember("Участник 1");
            var member2 = await CreateTestMember("Участник 2");
            
            await CreateTestEvent(member1, "Рейд 1");
            await CreateTestEvent(member1, "Рейд 2");
            await CreateTestEvent(member2, "Данж");
            var result = await _controller.GetEventsByMember(member1.Id);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var events = Assert.IsType<List<EventDTO>>(okResult.Value);
            
            Assert.Equal(2, events.Count);
            Assert.All(events, e => Assert.Equal(member1.Id, e.MemberId));
        }

        [Fact]
        public async Task UpdateEvent_WhenEventExists_UpdatesSuccessfully()
        {

            var member = await CreateTestMember();
            var eventEntity = await CreateTestEvent(member);
            
            var updateDto = new CreateEventDTO
            {
                Title = "Обновленное событие",
                Description = "Новое описание",
                Type = "Dungeon",
                StartDate = DateTime.Now.AddDays(2),
                MemberId = member.Id
            };

            var result = await _controller.PutEvent(eventEntity.Id, updateDto);

            Assert.IsType<NoContentResult>(result);
            var updatedEvent = await _context.Events.FindAsync(eventEntity.Id);
            Assert.NotNull(updatedEvent);
            Assert.Equal("Обновленное событие", updatedEvent.Title);
            Assert.Equal(EventType.Dungeon, updatedEvent.Type);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}