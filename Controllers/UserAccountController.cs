using System.Security.Cryptography;
using csharp_api.Data;
using csharp_api.Models.Entities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace csharp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccountController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public UserAccountController(AppDbContext dbContext) =>
            _dbContext = dbContext;

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

            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
            userAccount.Password = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: userAccount.Password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            await _dbContext.UserAccounts.AddAsync(userAccount);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = userAccount.Id }, userAccount);
        }
    }
}
