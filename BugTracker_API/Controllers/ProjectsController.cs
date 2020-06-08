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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ISharedService _service;

        public ProjectsController(DataContext context, IMapper mapper, ISharedService service)
        {
            _context = context;
            _mapper = mapper;
            _service = service;
        }

        // GET: api/Projects
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

        private bool ProjectExists(long id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
