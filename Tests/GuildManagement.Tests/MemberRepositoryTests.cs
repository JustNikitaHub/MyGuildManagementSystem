using Xunit;
using Microsoft.EntityFrameworkCore;
using GuildManagement.Repositories;
using GuildManagement.Entities;
using GuildManagement.Data;

namespace GuildManagement.Tests.Repositories
{
    public class MemberRepositoryTests : IDisposable
    {
        private readonly GuildManagementContext _context;
        private readonly MemberRepository _repository;

        public MemberRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<GuildManagementContext>()
                .UseInMemoryDatabase(databaseName: $"MemberRepoTestDb_{Guid.NewGuid()}")
                .Options;
            
            _context = new GuildManagementContext(options);
            _repository = new MemberRepository(_context);
            
            SeedTestData();
        }

        private void SeedTestData()
        {
            var members = new List<Member>
            {
                new Member("Артас Менетил", 60, MemberClass.Warrior),
                new Member("Джайна Праудмур", 58, MemberClass.Mage),
                new Member("Валера Сангуинар", 57, MemberClass.Rogue),
                new Member("Ульяна", 45, MemberClass.Mage),
                new Member("Борис", 30, MemberClass.Warrior)
            };

            _context.Members.AddRange(members);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllMembers()
        {
            var result = await _repository.GetAll();
            Assert.NotNull(result);
            Assert.Equal(5, result.Count());
        }

        [Fact]
        public async Task GetById_WhenMemberExists_ShouldReturnMember()
        {
            var member = _context.Members.First();
            var result = await _repository.GetById(member.Id);
            Assert.NotNull(result);
            Assert.Equal(member.Id, result.Id);
            Assert.Equal(member.Name, result.Name);
        }

        [Fact]
        public async Task GetById_WhenMemberNotExists_ShouldReturnNull()
        {
            var result = await _repository.GetById(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task Add_ShouldAddNewMember()
        {
            var newMember = new Member("Новый участник", 25, MemberClass.Rogue);
            var result = await _repository.Add(newMember);
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Новый участник", result.Name);
            
            var dbMember = await _context.Members.FindAsync(result.Id);
            Assert.NotNull(dbMember);
            Assert.Equal("Новый участник", dbMember.Name);
        }

        [Fact]
        public async Task Update_ShouldUpdateExistingMember()
        {
            var member = _context.Members.First();
            var originalName = member.Name;
            member.Name = "Обновленное имя";
            var result = await _repository.Update(member);
            Assert.NotNull(result);
            Assert.Equal("Обновленное имя", result.Name);
            var dbMember = await _context.Members.FindAsync(member.Id);
            Assert.NotNull(dbMember);
            Assert.Equal("Обновленное имя", dbMember.Name);
        }

        [Fact]
        public async Task Delete_ShouldRemoveMember()
        {
            var member = _context.Members.First();
            var memberId = member.Id;
            await _repository.Delete(memberId);
            var dbMember = await _context.Members.FindAsync(memberId);
            Assert.Null(dbMember);
        }

        [Fact]
        public async Task GetByLevelRange_ShouldReturnFilteredMembers()
        {
            var result = await _repository.GetByLevelRange(50, 60);
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.All(result, m => 
            {
                Assert.True(m.Level >= 50);
                Assert.True(m.Level <= 60);
            });
        }

        [Fact]
        public async Task GetByClass_ShouldReturnFilteredMembers()
        {
            var result = await _repository.GetByClass(MemberClass.Mage);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, m => Assert.Equal(MemberClass.Mage, m.MemberClass));
        }

        [Fact]
        public async Task GetByName_WhenMemberExists_ShouldReturnMember()
        {
            var result = await _repository.GetByName("Артас Менетил");
            Assert.NotNull(result);
            Assert.Equal("Артас Менетил", result.Name);
        }

        [Fact]
        public async Task GetByName_WhenMemberNotExists_ShouldReturnNull()
        {
            var result = await _repository.GetByName("Несуществующий");
            Assert.Null(result);
        }

        [Fact]
        public async Task Exists_WhenMemberExists_ShouldReturnTrue()
        {
            var member = _context.Members.First();
            var result = await _repository.Exists(member.Id);
            Assert.True(result);
        }

        [Fact]
        public async Task Exists_WhenMemberNotExists_ShouldReturnFalse()
        {
            var result = await _repository.Exists(999);
            Assert.False(result);
        }

        [Fact]
        public async Task ChangeLevel_ShouldUpdateMemberLevel()
        {
            var member = _context.Members.First();
            var newLevel = 55;
            var result = await _repository.ChangeLevel(member.Id, newLevel);
            Assert.True(result);
            var updatedMember = await _context.Members.FindAsync(member.Id);
            Assert.NotNull(updatedMember);
            Assert.Equal(newLevel, updatedMember.Level);
        }

        [Fact]
        public async Task ChangeLevel_WhenMemberNotExists_ShouldReturnFalse()
        {
            var result = await _repository.ChangeLevel(999, 60);
            Assert.False(result);
        }
    }
}