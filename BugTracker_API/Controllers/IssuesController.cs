////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Controllers\IssuesController.cs </file>
///
/// <copyright file="IssuesController.cs" company="MyCompany.com">
/// Copyright (c) 2020 MyCompany.com. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the issues controller class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugTracker_API.Data;
using AutoMapper;
using BugTracker_API.Models;
using Microsoft.AspNetCore.Authorization;
using BugTracker_API.Services;

namespace BugTracker_API.Controllers
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A controller for handling Issue requests. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    [Authorize]
    [Route("api/projects/{projectId:int}/[controller]")]
    [ApiController]
    public class IssuesController : ControllerBase
    {
        /// <summary>   The context. </summary>
        private readonly DataContext _context;
        /// <summary>   The mapper. </summary>
        private readonly IMapper _mapper;
        /// <summary>   The service. </summary>
        private readonly ISharedService _service;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="context">  The context. </param>
        /// <param name="mapper">   The mapper. </param>
        /// <param name="service">  The service. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public IssuesController(DataContext context, IMapper mapper, ISharedService service)
        {
            _context = context;
            _mapper = mapper;
            _service = service;
        }

        [AllowAnonymous]
        // GET: api/Issues

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   (An Action that handles HTTP GET requests) gets the issues. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        ///
        /// <returns>   An asynchronous result that yields the issues. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetIssueDto>>> GetIssues(long projectId)
        {
            if (!(await _service.GetProjectAsync(projectId) is Project project)) return NotFound("Project not found.");
            
            return await _context.Issues
                .Where(issue => issue.Project == project)
                .Include(i => i.Comments)
                .Include(i => i.User)
                .Select(i => _mapper.Map<GetIssueDto>(i))
                .ToListAsync();
        }

        [AllowAnonymous]
        // GET: api/Issues/5

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   (An Action that handles HTTP GET requests) gets an issue. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="number">       Number of issue. </param>
        ///
        /// <returns>   An asynchronous result that yields the issue. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpGet("{number}")]
        public async Task<ActionResult<GetIssueDto>> GetIssue(long projectId, long number)
        {
            if (!(await _service.GetProjectAsync(projectId) is Project project)) return NotFound("Project not found.");

            var issue = await _context.Issues
                .Where(issue => issue.Project == project)
                .Where(issue => issue.Number == number)
                .Include(issue => issue.Comments)
                .Include(issue => issue.User)
                .Select(issue => _mapper.Map<GetIssueDto>(issue))
                .SingleOrDefaultAsync();

            if (issue == null) return NotFound("Issue doesn't exist.");
            return issue;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   PUT: api/Issues/5. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <exception cref="DbUpdateConcurrencyException"> Thrown when a Database Update Concurrency
        ///                                                 error condition occurs. </exception>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="number">       Number of issue. </param>
        /// <param name="putIssue">     The put issue. </param>
        ///
        /// <returns>   An asynchronous result that yields an IActionResult. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPut("{number}")]
        public async Task<IActionResult> PutIssue(long projectId, long number, PutIssueDto putIssue)
        {
            if (!(await _service.GetProjectAsync(projectId) is Project project)) return NotFound("Project not found.");

            var issue = await _context.Issues
                .Where(issue => issue.Project == project)
                .Where(issue => issue.Number == number)
                .Include(issue => issue.User)
                .SingleOrDefaultAsync();
            
            if (issue == null) return BadRequest("Issue doesn't exist.");
            else if (_service.GetCurrentUserId() != issue.User.Id) return BadRequest("Only the creator of an Issue can modify it.");

            var id = issue.Id;

            _context.Entry(_mapper.Map(putIssue, issue)).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IssueExists(id))
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   POST: api/Issues. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="postIssue">    The post issue. </param>
        ///
        /// <returns>   An asynchronous result that yields an ActionResult&lt;GetIssueDto&gt; </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public async Task<ActionResult<GetIssueDto>> PostIssue(long projectId, PostIssueDto postIssue)
        {
            if (!(await _service.GetProjectAsync(projectId) is Project project)) return NotFound("Project not found.");

            var issue = _mapper.Map<Issue>(postIssue);

            issue.Project = project;
            issue.Number = await _context.Issues
                .Where(issue => issue.Project == project)
                .IgnoreQueryFilters()
                .CountAsync() + 1;
            issue.User = await _context.Users.FindAsync(_service.GetCurrentUserId());
            
            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIssue", new { number = issue.Number, projectId }, _mapper.Map<GetIssueDto>(issue));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   DELETE: api/Issues/5. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="number">       Number of issue. </param>
        ///
        /// <returns>   An asynchronous result that yields an ActionResult&lt;GetIssueDto&gt; </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpDelete("{number}")]
        public async Task<ActionResult<GetIssueDto>> DeleteIssue(long projectId, long number)
        {
            if (!(await _service.GetProjectAsync(projectId) is Project project)) return NotFound("Project not found.");

            var issue = await _context.Issues
                .Where(issue => issue.Project == project)
                .Where(issue => issue.Number == number)
                .Include(issue => issue.User)
                .SingleOrDefaultAsync();

            if (issue == null) return NotFound("Issue not found.");
            else if (_service.GetCurrentUserId() != issue.User.Id) return BadRequest("Only the creator of an Issue can delete it.");

            _context.Issues.Remove(issue);
            await _context.SaveChangesAsync();

            return _mapper.Map<GetIssueDto>(issue);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Query if 'id' issue exists. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="id">   The identifier. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private bool IssueExists(long id)
        {
            return _context.Issues.Any(e => e.Id == id);
        }
    }
}
