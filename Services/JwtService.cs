using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using csharp_api.Data;
using csharp_api.Models.Api;
using csharp_api.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace csharp_api.Services;

public class JwtService
{
    
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<UserAccount> _passwordHasher;

    public JwtService(AppDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<UserAccount>();
    }

    public async Task<LoginResponseModel?> Authenticate(LoginRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.Password))
            return null;

        var userAccount = await _dbContext.UserAccounts.FirstOrDefaultAsync(x => x.UserName == request.UserName);
        if (userAccount is null || _passwordHasher.VerifyHashedPassword(userAccount, userAccount.Password, request.Password) == 0) // TODO: need to write code for edge case == 2
            return null;
        
        var issuer = _configuration["Issuer"];
        var audience = _configuration["Audience"];
        var key = _configuration["Key"];
        int tokenValidityMins;
        if (!int.TryParse(_configuration["TokenValidityMins"], out tokenValidityMins)) {
            tokenValidityMins = 60;
        }
        var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Name, request.UserName)
            }),
            Expires = tokenExpiryTimeStamp,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key ?? "")),
                SecurityAlgorithms.HmacSha512Signature),
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);

        return new LoginResponseModel
        {
            AccessToken = accessToken,
            UserName = request.UserName,
            ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.UtcNow).TotalSeconds
        };
    }
}
