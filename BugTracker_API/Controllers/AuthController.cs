////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Controllers\AuthController.cs </file>
///
/// <copyright file="AuthController.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the authentication controller class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

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
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A controller for handling authentication. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>   The authentication repo. </summary>
        private readonly IAuthRepository _authRepo;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="authRepository">   The authentication repository. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public AuthController(IAuthRepository authRepository)
        {
            _authRepo = authRepository;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   (An Action that handles HTTP POST requests) Registers the user. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="request">  The request. </param>
        ///
        /// <returns>   An asynchronous result that yields an IActionResult. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            try
            {
                return Ok(new { id = await _authRepo.Register(new User { Username = request.Username }, request.Password) });
            } catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   (An Action that handles HTTP POST requests) Login. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="request">  The request. </param>
        ///
        /// <returns>   An asynchronous result that yields an IActionResult. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            try
            {
                return Ok(new { token = await _authRepo.Login(request.Username, request.Password) });
            } catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
