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
    public class AchievementsControllerSimpleIntegrationTests : IDisposable
    {
        private readonly GuildManagementContext _context;
        private readonly AchievementsController _controller;

        public AchievementsControllerSimpleIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<GuildManagementContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            
            _context = new GuildManagementContext(options);
            _context.Database.EnsureCreated();
            
            _controller = new AchievementsController(_context);
        }

        private async Task<Member> CreateTestMember(string name = "Тестовый участник")
        {
            var member = new Member(name, 50, MemberClass.Warrior);
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        private async Task<Achievement> CreateTestAchievement(Member member, 
            string title = "Тестовое достижение",
            string description = "Описание достижения")
        {
            var achievement = new Achievement(title, description, member.Id)
            {
                Member = member
            };
            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();
            return achievement;
        }

        [Fact]
        public async Task GetAllAchievements_ReturnsOkResult_WithAchievementsList()
        {
            
            var member = await CreateTestMember();
            await CreateTestAchievement(member, "Покоритель Наксрамаса", "Пройден рейд Наксрамас");
            await CreateTestAchievement(member, "Мастер заданий", "Выполнено 100 заданий");
            var result = await _controller.GetAchievements();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var achievements = Assert.IsType<List<AchievementDTO>>(okResult.Value);
            Assert.Equal(2, achievements.Count);
        }

        [Fact]
        public async Task CreateAchievement_WithValidData_ReturnsCreatedAchievement()
        {
            var member = await CreateTestMember();
            
            var newAchievement = new CreateAchievementDTO
            {
                Title = "Новое достижение",
                Description = "Описание нового достижения",
                MemberId = member.Id
            };
            var result = await _controller.PostAchievement(newAchievement);
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var achievementDto = Assert.IsType<AchievementDTO>(createdAtResult.Value);
            Assert.Equal("Новое достижение", achievementDto.Title);
            Assert.Equal("Описание нового достижения", achievementDto.Description);
            Assert.Equal(member.Id, achievementDto.MemberId);
            var dbAchievement = await _context.Achievements.FindAsync(achievementDto.Id);
            Assert.NotNull(dbAchievement);
            Assert.Equal("Новое достижение", dbAchievement.Title);
        }

        [Fact]
        public async Task GetAchievementsByMember_ReturnsMemberAchievements()
        {
            var member1 = await CreateTestMember("Участник 1");
            var member2 = await CreateTestMember("Участник 2");
            await CreateTestAchievement(member1, "Достижение 1", "Описание 1");
            await CreateTestAchievement(member1, "Достижение 2", "Описание 2");
            await CreateTestAchievement(member2, "Достижение 3", "Описание 3");
            var result = await _controller.GetAchievementsByMember(member1.Id);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var achievements = Assert.IsType<List<AchievementDTO>>(okResult.Value);
            Assert.Equal(2, achievements.Count);
            Assert.All(achievements, a => Assert.Equal(member1.Id, a.MemberId));
        }

        [Fact]
        public async Task UpdateAchievement_WhenAchievementExists_UpdatesSuccessfully()
        {
            var member = await CreateTestMember();
            var achievement = await CreateTestAchievement(member);
            var updateDto = new CreateAchievementDTO
            {
                Title = "Обновленное достижение",
                Description = "Новое описание достижения",
                MemberId = member.Id
            };
            var result = await _controller.PutAchievement(achievement.Id, updateDto);
            Assert.IsType<NoContentResult>(result);
            var updatedAchievement = await _context.Achievements.FindAsync(achievement.Id);
            Assert.NotNull(updatedAchievement);
            Assert.Equal("Обновленное достижение", updatedAchievement.Title);
            Assert.Equal("Новое описание достижения", updatedAchievement.Description);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}