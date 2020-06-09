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
    [Authorize]
    [Route("api/projects/{projectId:int}/[controller]")]
    [ApiController]
    public class IssuesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ISharedService _service;

        public IssuesController(DataContext context, IMapper mapper, ISharedService service)
        {
            _context = context;
            _mapper = mapper;
            _service = service;
        }

        [AllowAnonymous]
        // GET: api/Issues
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
        [HttpGet("{id}")]
        public async Task<ActionResult<GetIssueDto>> GetIssue(long projectId, long id)
        {
            if (!(await _service.GetProjectAsync(projectId) is Project project)) return NotFound("Project not found.");

            var issue = await _context.Issues
                .Where(issue => issue.Project == project)
                .Where(issue => issue.Id == id)
                .Include(issue => issue.Comments)
                .Include(issue => issue.User)
                .Select(issue => _mapper.Map<GetIssueDto>(issue))
                .SingleOrDefaultAsync();

            if (issue == null) return NotFound("Issue doesn't exist.");
            return issue;
        }

        // PUT: api/Issues/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIssue(long projectId, long id, PutIssueDto putIssue)
        {
            if (!(await _service.GetProjectAsync(projectId) is Project project)) return NotFound("Project not found.");

            var issue = await _context.Issues
                .Where(issue => issue.Project == project)
                .Where(issue => issue.Id == id)
                .Include(issue => issue.User)
                .SingleOrDefaultAsync();
            
            if (issue == null) return BadRequest("Issue doesn't exist.");
            else if (_service.GetCurrentUserId() != issue.User.Id) return BadRequest("Only the creator of an Issue can modify it.");

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

        // POST: api/Issues
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

            return CreatedAtAction("GetIssue", new { id = issue.Id, projectId }, _mapper.Map<GetIssueDto>(issue));
        }

        // DELETE: api/Issues/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<GetIssueDto>> DeleteIssue(long projectId, long id)
        {
            if (!(await _service.GetProjectAsync(projectId) is Project project)) return NotFound("Project not found.");

            var issue = await _context.Issues
                .Where(issue => issue.Project == project)
                .Where(issue => issue.Id == id)
                .Include(issue => issue.User)
                .SingleOrDefaultAsync();

            if (issue == null) return NotFound("Issue not found.");
            else if (_service.GetCurrentUserId() != issue.User.Id) return BadRequest("Only the creator of an Issue can delete it.");

            _context.Issues.Remove(issue);
            await _context.SaveChangesAsync();

            return _mapper.Map<GetIssueDto>(issue);
        }

        private bool IssueExists(long id)
        {
            return _context.Issues.Any(e => e.Id == id);
        }
    }
}
