////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Controllers\ProjectsController.cs </file>
///
/// <copyright file="ProjectsController.cs" company="MyCompany.com">
/// Copyright (c) 2020 MyCompany.com. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the projects controller class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugTracker_API.Data;
using AutoMapper;
using BugTracker_API.Services;
using Microsoft.AspNetCore.Authorization;

namespace BugTracker_API.Controllers
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A controller for handling projects. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
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

        public ProjectsController(DataContext context, IMapper mapper, ISharedService service)
        {
            _context = context;
            _mapper = mapper;
            _service = service;
        }

        // GET: api/Projects

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   (An Action that handles HTTP GET requests) gets the projects. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <returns>   An asynchronous result that yields the projects. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetProjectDto>>> GetProjects()
        {
            return await _context.Projects
                .Include(project => project.Issues)
                .Include(project => project.User)
                .Select(project => _mapper.Map<GetProjectDto>(project))
                .ToListAsync();
        }

        // GET: api/Projects/5

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   (An Action that handles HTTP GET requests) gets a project. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="id">   The identifier. </param>
        ///
        /// <returns>   An asynchronous result that yields the project. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<GetProjectDto>> GetProject(long id)
        {
            var project = await _context.Projects
                .Where(project => project.Id == id)
                .Include(project => project.Issues)
                .Include(project => project.User)
                .Select(project => _mapper.Map<GetProjectDto>(project))
                .SingleOrDefaultAsync();

            if (project == null) return NotFound("Project doesn't exist.");
            return project;
        }

        // PUT: api/Projects/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   (An Action that handles HTTP PUT requests) puts a project. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <exception cref="DbUpdateConcurrencyException"> Thrown when a Database Update Concurrency
        ///                                                 error condition occurs. </exception>
        ///
        /// <param name="id">           The identifier. </param>
        /// <param name="putProject">   The put project. </param>
        ///
        /// <returns>   An asynchronous result that yields an IActionResult. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(long id, PutProjectDto putProject)
        {
            var project = await _context.Projects
                .Where(project => project.Id == id)
                .Include(project => project.User)
                .SingleOrDefaultAsync();

            if (project == null) return BadRequest("Project doesn't exist.");
            else if (_service.GetCurrentUserId() != project.User.Id) return BadRequest("Only the creator of a Project can modify it.");

            _context.Entry(_mapper.Map(putProject, project)).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
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

        // POST: api/Projects
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   (An Action that handles HTTP POST requests) posts a project. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="postProject">  The post project. </param>
        ///
        /// <returns>   An asynchronous result that yields an ActionResult&lt;GetProjectDto&gt; </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public async Task<ActionResult<GetProjectDto>> PostProject(PostProjectDto postProject)
        {
            var project = _mapper.Map<Project>(postProject);

            project.User = await _context.Users.FindAsync(_service.GetCurrentUserId());

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProject", new { id = project.Id }, _mapper.Map<GetProjectDto>(project));
        }

        // DELETE: api/Projects/5

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// (An Action that handles HTTP DELETE requests) deletes the project described by ID.
        /// </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="id">   The identifier. </param>
        ///
        /// <returns>   An asynchronous result that yields an ActionResult&lt;GetProjectDto&gt; </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpDelete("{id}")]
        public async Task<ActionResult<GetProjectDto>> DeleteProject(long id)
        {
            var project = await _context.Projects
                .Where(project => project.Id == id)
                .Include(project => project.User)
                .SingleOrDefaultAsync();

            if (project == null) return NotFound("Project not found.");
            else if (_service.GetCurrentUserId() != project.User.Id) return BadRequest("Only the creator of a Project can delete it.");


            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return _mapper.Map<GetProjectDto>(project);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Queries if a given project exists. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="id">   The identifier. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private bool ProjectExists(long id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
