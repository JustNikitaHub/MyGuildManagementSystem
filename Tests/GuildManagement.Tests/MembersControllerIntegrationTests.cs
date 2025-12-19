using Xunit;
using Microsoft.AspNetCore.Mvc;
using GuildManagement.Controllers;
using GuildManagement.Data;
using GuildManagement.DTOs;
using Microsoft.EntityFrameworkCore;
using GuildManagement.Entities;


namespace GuildManagement.Tests.Controllers
{
    public class MembersControllerSimpleIntegrationTests : IDisposable
    {
        private readonly GuildManagementContext _context;
        private readonly MembersController _controller;

        public MembersControllerSimpleIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<GuildManagementContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            
            _context = new GuildManagementContext(options);
            _context.Database.EnsureCreated();
            
            _controller = new MembersController(_context);
        }

        private async Task SeedTestData()
        {
            var members = new List<Member>
            {
                new Member("Артас Менетил", 60, MemberClass.Warrior),
                new Member("Джайна Праудмур", 58, MemberClass.Mage),
                new Member("Валера Сангуинар", 57, MemberClass.Rogue),
                new Member("Воин", 45, MemberClass.Warrior),
                new Member("Маг", 50, MemberClass.Mage)
            };

            await _context.Members.AddRangeAsync(members);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllMembers_ReturnsOkResult_WithMembersList()
        {
            await SeedTestData();
            var result = await _controller.GetMembers();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var members = Assert.IsType<List<MemberDTO>>(okResult.Value);
            Assert.Equal(5, members.Count);
            Assert.Contains(members, m => m.Name == "Артас Менетил");
            Assert.Contains(members, m => m.Name == "Джайна Праудмур");
        }

        [Fact]
        public async Task GetMemberById_WhenMemberExists_ReturnsMember()
        {
            await SeedTestData();
            var member = _context.Members.First(m => m.Name == "Артас Менетил");
            var result = await _controller.GetMember(member.Id);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var memberDto = Assert.IsType<MemberDTO>(okResult.Value);
            Assert.Equal(member.Id, memberDto.Id);
            Assert.Equal("Артас Менетил", memberDto.Name);
            Assert.Equal(60, memberDto.Level);
            Assert.Equal("Warrior", memberDto.MemberClass);
        }

        [Fact]
        public async Task GetMemberById_WhenMemberNotExists_ReturnsNotFound()
        {
            await SeedTestData();
            var result = await _controller.GetMember(999);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateMember_WithValidData_ReturnsCreatedMember()
        {
            await SeedTestData();
            var newMember = new CreateMemberDTO
            {
                Name = "Новый участник",
                Level = 25,
                MemberClass = "Rogue"
            };
            var result = await _controller.PostMember(newMember);
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var memberDto = Assert.IsType<MemberDTO>(createdAtResult.Value);
            
            Assert.Equal("Новый участник", memberDto.Name);
            Assert.Equal(25, memberDto.Level);
            Assert.Equal("Rogue", memberDto.MemberClass);
            

            var dbMember = await _context.Members.FindAsync(memberDto.Id);
            Assert.NotNull(dbMember);
            Assert.Equal("Новый участник", dbMember.Name);
        }

        [Fact]
        public async Task UpdateMember_WhenMemberExists_UpdatesSuccessfully()
        {

            await SeedTestData();
            var existingMember = _context.Members.First(m => m.Name == "Артас Менетил");
            
            var updateDto = new CreateMemberDTO
            {
                Name = "Обновленный Артас",
                Level = 61,
                MemberClass = "Warrior"
            };
            var result = await _controller.PutMember(existingMember.Id, updateDto);
            Assert.IsType<NoContentResult>(result);
            var updatedMember = await _context.Members.FindAsync(existingMember.Id);
            Assert.NotNull(updatedMember);
            Assert.Equal("Обновленный Артас", updatedMember.Name);
            Assert.Equal(61, updatedMember.Level);
        }

        [Fact]
        public async Task DeleteMember_WhenMemberExists_DeletesSuccessfully()
        {
            await SeedTestData();
            var member = _context.Members.First(m => m.Name == "Артас Менетил");
            var result = await _controller.DeleteMember(member.Id);
            Assert.IsType<NoContentResult>(result);
            var dbMember = await _context.Members.FindAsync(member.Id);
            Assert.Null(dbMember);
        }

        [Fact]
        public async Task GetMembersByClass_ReturnsFilteredMembers()
        {
            await SeedTestData();
            var result = await _controller.GetMembersByClass("Warrior");
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var members = Assert.IsType<List<MemberDTO>>(okResult.Value);
            Assert.Equal(2, members.Count);
            Assert.All(members, m => Assert.Equal("Warrior", m.MemberClass));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}