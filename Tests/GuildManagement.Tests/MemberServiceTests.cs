using Xunit;
using Moq;
using GuildManagement.Services;
using GuildManagement.Interfaces;
using GuildManagement.Entities;
using Microsoft.EntityFrameworkCore;
using GuildManagement.Data;

namespace GuildManagement.Tests.Services
{
    public class MemberServiceTests : IDisposable
    {
        private readonly Mock<IMemberRepository> _memberRepositoryMock;
        private readonly GuildManagementContext _context;
        private readonly MemberService _memberService;

        public MemberServiceTests()
        {
            _memberRepositoryMock = new Mock<IMemberRepository>();
            var options = new DbContextOptionsBuilder<GuildManagementContext>()
                .UseInMemoryDatabase(databaseName: $"MemberTestDb_{Guid.NewGuid()}")
                .Options;
            _context = new GuildManagementContext(options);
            _memberService = new MemberService(_memberRepositoryMock.Object, _context);
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllMembers_ShouldReturnAllMembers()
        {
            var expectedMembers = new List<Member>
            {
                new Member("Артас Менетил", 60, MemberClass.Warrior) { Id = 1 },
                new Member("Джайна Праудмур", 58, MemberClass.Mage) { Id = 2 }
            };
            _memberRepositoryMock
                .Setup(repo => repo.GetAll())
                .ReturnsAsync(expectedMembers);
            var result = await _memberService.GetAllMembers();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Артас Менетил", result[0].Name);
            _memberRepositoryMock.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetMemberById_WhenMemberExists_ShouldReturnMember()
        {
            var expectedMember = new Member("Артас Менетил", 60, MemberClass.Warrior) 
            { 
                Id = 1 
            };
            _memberRepositoryMock
                .Setup(repo => repo.GetById(1))
                .ReturnsAsync(expectedMember);
            var result = await _memberService.GetMemberById(1);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Артас Менетил", result.Name);
            _memberRepositoryMock.Verify(repo => repo.GetById(1), Times.Once);
        }

        [Fact]
        public async Task CreateMember_WithValidData_ShouldReturnCreatedMember()
        {
            var newMember = new Member("Новый участник", 25, MemberClass.Rogue);
            var createdMember = new Member("Новый участник", 25, MemberClass.Rogue) 
            { 
                Id = 100 
            };
            _memberRepositoryMock
                .Setup(repo => repo.Add(It.IsAny<Member>()))
                .ReturnsAsync(createdMember);
            var result = await _memberService.CreateMember(newMember);
            Assert.NotNull(result);
            Assert.Equal(100, result.Id);
            Assert.Equal("Новый участник", result.Name);
            _memberRepositoryMock.Verify(repo => repo.Add(It.IsAny<Member>()), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(61)]
        public async Task CreateMember_WithInvalidLevel_ShouldThrowArgumentException(int invalidLevel)
        {
            var invalidMember = new Member("Новый участник", invalidLevel, MemberClass.Rogue);
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _memberService.CreateMember(invalidMember));
        }
    }
}