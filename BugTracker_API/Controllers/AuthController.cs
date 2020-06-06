using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugTracker_API.Data;
using BugTracker_API.Helpers;

namespace BugTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepo = authRepository;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            var id = await _authRepo.Register(new User { Username = request.Username }, request.Password);
            if (id < 1)
            {
                return BadRequest();
            }
            return Ok();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            try
            {
                return Ok(await _authRepo.Login(request.Username, request.Password));
            } catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
