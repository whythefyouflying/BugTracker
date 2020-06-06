using AutoMapper.Configuration;
using BugTracker_API;
using BugTracker_API.Data;
using BugTracker_API.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

public class AuthRepository : IAuthRepository
{
    private readonly DataContext _context;
    private readonly SecretKey _secret;

    public AuthRepository(DataContext context, IOptions<SecretKey> secret)
    {
        _context = context;
        _secret = secret.Value;
    }

    public async Task<string> Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new AppException("Password is required");

        User user = await _context.Users.FirstOrDefaultAsync(x => x.Username.ToLower().Equals(username.ToLower()));
        if (user == null)
        {
            throw new AppException("Username '" + username + "' doesn't exist");
        }
        else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) 
        {
            throw new AppException("Incorrect password.");
        }

        return CreateToken(user);
    }

    public async Task<int> Register(User user, string password)
    {
        if (await UserExists(user.Username))
        {
            // TODO: make this throw an AppException for example
            return 0;
        }
        CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user.Id;
    }

    public async Task<bool> UserExists(string username)
    {
        if (await _context.Users.AnyAsync(x => x.Username.ToLower() == username.ToLower()))
        {
            return true;
        }
        return false;
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new System.Security.Cryptography.HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
        {
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != passwordHash[i])
                {
                    return false;
                }
            }
            return true;
        }
    }

    private string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(_secret.Key));

        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = creds
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}