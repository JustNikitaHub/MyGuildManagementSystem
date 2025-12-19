using Xunit;
using Microsoft.AspNetCore.Mvc;
using GuildManagement.Controllers;
using GuildManagement.Data;
using GuildManagement.DTOs;
using Microsoft.EntityFrameworkCore;
using GuildManagement.Entities;

namespace GuildManagement.Tests.Controllers
{
    public class ResourcesControllerSimpleIntegrationTests : IDisposable
    {
        private readonly GuildManagementContext _context;
        private readonly ResourcesController _controller;

        public ResourcesControllerSimpleIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<GuildManagementContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            
            _context = new GuildManagementContext(options);
            _context.Database.EnsureCreated();
            
            _controller = new ResourcesController(_context);
        }

        private async Task<Member> CreateTestMember(string name = "Тестовый участник")
        {
            var member = new Member(name, 50, MemberClass.Warrior);
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        private async Task<Resource> CreateTestResource(Member member, 
            string name = "Золото", 
            ResourceType type = ResourceType.Gold,
            int quantity = 100)
        {
            var resource = new Resource(name, type, quantity, member.Id)
            {
                Member = member
            };
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();
            return resource;
        }

        [Fact]
        public async Task GetAllResources_ReturnsOkResult_WithResourcesList()
        {
            var member = await CreateTestMember();
            await CreateTestResource(member, "Золото", ResourceType.Gold, 100);
            await CreateTestResource(member, "Железо", ResourceType.Iron, 200);
            var result = await _controller.GetResources();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var resources = Assert.IsType<List<ResourceDTO>>(okResult.Value);
            Assert.Equal(2, resources.Count);
        }

        [Fact]
        public async Task CreateResource_WithValidData_ReturnsCreatedResource()
        {
            var member = await CreateTestMember();
            var newResource = new CreateResourceDTO
            {
                Name = "Мифрил",
                Type = "Iron",
                Quantity = 500,
                Rarity = "Rare",
                Description = "Ценный мифрил",
                MemberId = member.Id
            };
            var result = await _controller.PostResource(newResource);
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var resourceDto = Assert.IsType<ResourceDTO>(createdAtResult.Value);
            Assert.Equal("Мифрил", resourceDto.Name);
            Assert.Equal("Iron", resourceDto.Type);
            Assert.Equal(500, resourceDto.Quantity);
            Assert.Equal("Rare", resourceDto.Rarity);
            var dbResource = await _context.Resources.FindAsync(resourceDto.Id);
            Assert.NotNull(dbResource);
            Assert.Equal("Мифрил", dbResource.Name);
        }

        [Fact]
        public async Task GetResourcesByMember_ReturnsMemberResources()
        {
            var member1 = await CreateTestMember("Участник 1");
            var member2 = await CreateTestMember("Участник 2");
            await CreateTestResource(member1, "Золото 1", ResourceType.Gold, 100);
            await CreateTestResource(member1, "Золото 2", ResourceType.Gold, 200);
            await CreateTestResource(member2, "Железо", ResourceType.Iron, 300);
            var result = await _controller.GetResourcesByMember(member1.Id);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var resources = Assert.IsType<List<ResourceDTO>>(okResult.Value);
            Assert.Equal(2, resources.Count);
            Assert.All(resources, r => Assert.Equal(member1.Id, r.MemberId));
        }

        [Fact]
        public async Task UpdateResourceQuantity_UpdatesQuantitySuccessfully()
        {
            var member = await CreateTestMember();
            var resource = await CreateTestResource(member, "Золото", ResourceType.Gold, 100);
            var result = await _controller.UpdateResourceQuantity(resource.Id, 500);
            Assert.IsType<NoContentResult>(result);
            var updatedResource = await _context.Resources.FindAsync(resource.Id);
            Assert.NotNull(updatedResource);
            Assert.Equal(500, updatedResource.Quantity);
        }

        [Fact]
        public async Task GetResourcesByType_ReturnsFilteredResources()
        {
            var member = await CreateTestMember();
            await CreateTestResource(member, "Золото 1", ResourceType.Gold, 100);
            await CreateTestResource(member, "Золото 2", ResourceType.Gold, 200);
            await CreateTestResource(member, "Железо", ResourceType.Iron, 300);
            var result = await _controller.GetResourcesByType("Gold");
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var resources = Assert.IsType<List<ResourceDTO>>(okResult.Value);
            Assert.Equal(2, resources.Count);
            Assert.All(resources, r => Assert.Equal("Gold", r.Type));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}