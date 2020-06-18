////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Data\AuthRepository.cs </file>
///
/// <copyright file="AuthRepository.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the authentication repository class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using AutoMapper.Configuration;
using BugTracker_API;
using BugTracker_API.Data;
using BugTracker_API.Models;
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

namespace BugTracker_API.Data
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   An authentication repository. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class AuthRepository : IAuthRepository
    {
        /// <summary>   The context. </summary>
        private readonly DataContext _context;
        /// <summary>   The secret. </summary>
        private readonly SecretKey _secret;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="context">  The context. </param>
        /// <param name="secret">   The secret. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public AuthRepository(DataContext context, IOptions<SecretKey> secret)
        {
            _context = context;
            _secret = secret.Value;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Login. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <exception cref="AppException"> Thrown when an Application error condition occurs. </exception>
        ///
        /// <param name="username"> The username. </param>
        /// <param name="password"> The password. </param>
        ///
        /// <returns>   An asynchronous result that yields a bearer token string. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<string> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new AppException("Username is required.");
            else if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required.");

            User user = await _context.Users.FirstOrDefaultAsync(x => x.Username.ToLower().Equals(username.ToLower()));
            if (user == null)
                throw new AppException("Username '" + username + "' doesn't exist.");
            else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) 
                throw new AppException("Incorrect password.");

            return CreateToken(user);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Registers this user. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <exception cref="AppException"> Thrown when an Application error condition occurs. </exception>
        ///
        /// <param name="user">     The user. </param>
        /// <param name="password"> The password. </param>
        ///
        /// <returns>   An asynchronous result that yields a user Id. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<int> Register(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(user.Username))
                throw new AppException("Username is required.");
            else if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required.");

            if (await UserExists(user.Username))
                throw new AppException("Username '" + user.Username + "' already exists.");

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user.Id;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Queries if a given user exists. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="username"> The username. </param>
        ///
        /// <returns>   An asynchronous result that yields true if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<bool> UserExists(string username)
        {
            if (await _context.Users.AnyAsync(x => x.Username.ToLower() == username.ToLower()))
            {
                return true;
            }
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Creates password hash. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="password">     The password. </param>
        /// <param name="passwordHash"> [out] The password hash. </param>
        /// <param name="passwordSalt"> [out] The password salt. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Verify password hash. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="password">     The password. </param>
        /// <param name="passwordHash"> The password hash. </param>
        /// <param name="passwordSalt"> The password salt. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt);
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Creates a token. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="user"> The user. </param>
        ///
        /// <returns>   The new token. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

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
}
