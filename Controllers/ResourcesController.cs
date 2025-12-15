using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GuildManagement.Data;
using GuildManagement.Entities;
using GuildManagement.DTOs;

namespace GuildManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourcesController : ControllerBase
    {
        private readonly GuildManagementContext _context;

        public ResourcesController(GuildManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResourceDTO>>> GetResources()
        {
            var resources = await _context.Resources
                .Include(r => r.Member)
                .ToListAsync();

            var resourceDTOs = resources.Select(r => new ResourceDTO
            {
                Id = r.Id,
                Name = r.Name,
                Type = r.Type.ToString(),
                Quantity = r.Quantity,
                Rarity = r.Rarity.ToString(),
                Description = r.Description,
                MemberId = r.MemberId,
                MemberName = r.Member?.Name
            }).ToList();

            return Ok(resourceDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResourceDTO>> GetResource(int id)
        {
            var resource = await _context.Resources
                .Include(r => r.Member)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resource == null)
            {
                return NotFound();
            }

            var resourceDTO = new ResourceDTO
            {
                Id = resource.Id,
                Name = resource.Name,
                Type = resource.Type.ToString(),
                Quantity = resource.Quantity,
                Rarity = resource.Rarity.ToString(),
                Description = resource.Description,
                MemberId = resource.MemberId,
                MemberName = resource.Member?.Name
            };

            return Ok(resourceDTO);
        }

        [HttpPost]
        public async Task<ActionResult<ResourceDTO>> PostResource(CreateResourceDTO createResourceDTO)
        {
            var member = await _context.Members.FindAsync(createResourceDTO.MemberId);
            if (member == null)
            {
                return BadRequest("Участник не найден");
            }

            if (!Enum.TryParse<ResourceType>(createResourceDTO.Type, out var resourceType))
            {
                return BadRequest("Неверный тип ресурса");
            }

            Rarity? rarity = null;
            if (!string.IsNullOrEmpty(createResourceDTO.Rarity))
            {
                if (!Enum.TryParse<Rarity>(createResourceDTO.Rarity, out var parsedRarity))
                {
                    return BadRequest("Неверная редкость ресурса");
                }
                rarity = parsedRarity;
            }

            var resource = new Resource
            {
                Name = createResourceDTO.Name,
                Type = resourceType,
                Quantity = createResourceDTO.Quantity,
                Rarity = rarity ?? Rarity.Common,
                Description = createResourceDTO.Description,
                MemberId = createResourceDTO.MemberId
            };

            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();

            var resourceDTO = new ResourceDTO
            {
                Id = resource.Id,
                Name = resource.Name,
                Type = resource.Type.ToString(),
                Quantity = resource.Quantity,
                Rarity = resource.Rarity.ToString(),
                Description = resource.Description,
                MemberId = resource.MemberId,
                MemberName = member.Name
            };

            return CreatedAtAction(nameof(GetResource), new { id = resource.Id }, resourceDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutResource(int id, CreateResourceDTO createResourceDTO)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(createResourceDTO.MemberId);
            if (member == null)
            {
                return BadRequest("Участник не найден");
            }

            if (!Enum.TryParse<ResourceType>(createResourceDTO.Type, out var resourceType))
            {
                return BadRequest("Неверный тип ресурса");
            }

            Rarity? rarity = null;
            if (!string.IsNullOrEmpty(createResourceDTO.Rarity))
            {
                if (!Enum.TryParse<Rarity>(createResourceDTO.Rarity, out var parsedRarity))
                {
                    return BadRequest("Неверная редкость ресурса");
                }
                rarity = parsedRarity;
            }

            resource.Name = createResourceDTO.Name;
            resource.Type = resourceType;
            resource.Quantity = createResourceDTO.Quantity;
            resource.Rarity = rarity ?? resource.Rarity;
            resource.Description = createResourceDTO.Description;
            resource.MemberId = createResourceDTO.MemberId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResourceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResource(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null)
            {
                return NotFound();
            }

            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<IEnumerable<ResourceDTO>>> GetResourcesByMember(int memberId)
        {
            var resources = await _context.Resources
                .Where(r => r.MemberId == memberId)
                .ToListAsync();

            var resourceDTOs = resources.Select(r => new ResourceDTO
            {
                Id = r.Id,
                Name = r.Name,
                Type = r.Type.ToString(),
                Quantity = r.Quantity,
                Rarity = r.Rarity.ToString(),
                Description = r.Description,
                MemberId = r.MemberId
            }).ToList();

            return Ok(resourceDTOs);
        }
        [HttpPut("{id}/quantity")]
        public async Task<IActionResult> UpdateResourceQuantity(int id, [FromBody] int newQuantity)
        {
            if (newQuantity < 0)
            {
                return BadRequest("Количество не может быть отрицательным");
            }

            var resource = await _context.Resources.FindAsync(id);
            if (resource == null)
            {
                return NotFound();
            }

            resource.Quantity = newQuantity;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<ResourceDTO>>> GetResourcesByType(string type)
        {
            if (!Enum.TryParse<ResourceType>(type, out var resourceType))
            {
                return BadRequest("Неверный тип ресурса");
            }

            var resources = await _context.Resources
                .Where(r => r.Type == resourceType)
                .Include(r => r.Member)
                .ToListAsync();

            var resourceDTOs = resources.Select(r => new ResourceDTO
            {
                Id = r.Id,
                Name = r.Name,
                Type = r.Type.ToString(),
                Quantity = r.Quantity,
                Rarity = r.Rarity.ToString(),
                Description = r.Description,
                MemberId = r.MemberId,
                MemberName = r.Member?.Name
            }).ToList();

            return Ok(resourceDTOs);
        }

        private bool ResourceExists(int id)
        {
            return _context.Resources.Any(e => e.Id == id);
        }
    }
}