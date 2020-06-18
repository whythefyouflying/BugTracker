////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Services\SharedService.cs </file>
///
/// <copyright file="SharedService.cs" company="MyCompany.com">
/// Copyright (c) 2020 MyCompany.com. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the shared service class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using BugTracker_API.Data;
using BugTracker_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BugTracker_API.Services
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A service for accessing shared information. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class SharedService : ISharedService
    {
        /// <summary>   The context. </summary>
        private readonly DataContext _context;
        /// <summary>   The HTTP context accessor. </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="context">              The context. </param>
        /// <param name="httpContextAccessor">  The HTTP context accessor. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public SharedService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets issue asynchronous. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="project">      The project. </param>
        /// <param name="issueNumber">  The issue number. </param>
        ///
        /// <returns>   An asynchronous result that yields the issue. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<Issue> GetIssueAsync(Project project, int issueNumber)
        {
            return await _context.Issues
                .Where(issue => issue.Project == project)
                .Where(issue => issue.Number == issueNumber)
                .Include(issue => issue.User)
                .SingleOrDefaultAsync();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets current user identifier. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <returns>   The current user identifier. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public int GetCurrentUserId()
        {
            return int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets project asynchronous. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        ///
        /// <returns>   An asynchronous result that yields the project. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<Project> GetProjectAsync(long projectId)
        {
            return await _context.Projects
                .Where(project => project.Id == projectId)
                .Include(project => project.User)
                .SingleOrDefaultAsync();
        }
    }
}
