using csharp_api.Data;
using csharp_api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace csharp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserAccountController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly PasswordHasher<UserAccount> _passwordHasher;

        public UserAccountController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _passwordHasher = new PasswordHasher<UserAccount>();
        }

        [HttpGet]
        public async Task<List<UserAccount>> Get()
        {
            return await _dbContext.UserAccounts.ToListAsync();
        }

        [HttpGet]
        public async Task<UserAccount?> GetById(Guid id)
        {
            return await _dbContext.UserAccounts.FirstOrDefaultAsync(x => x.Id == id);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] UserAccount userAccount)
        {
            if (string.IsNullOrWhiteSpace(userAccount.UserName) ||
                string.IsNullOrWhiteSpace(userAccount.Password))
            {
                return BadRequest("Invalid Request");
            }

            userAccount.Password = _passwordHasher.HashPassword(userAccount, userAccount.Password);

            await _dbContext.UserAccounts.AddAsync(userAccount);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = userAccount.Id }, userAccount);
        }
    }
}
