using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugTracker_API.Data;
using AutoMapper;
using BugTracker_API.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BugTracker_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssuesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public IssuesController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [AllowAnonymous]
        // GET: api/Issues
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetIssueDto>>> GetIssues()
        {
            return await _context.Issues.Include(i => i.Comments).Include(i => i.User).Select(i => _mapper.Map<GetIssueDto>(i)).ToListAsync();
        }

        [AllowAnonymous]
        // GET: api/Issues/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetIssueDto>> GetIssue(long id)
        {
            var issue = _mapper.Map<GetIssueDto>(await _context.Issues.Include(i => i.Comments).Include(i => i.User).SingleOrDefaultAsync(i => i.Id == id));

            if (issue == null)
            {
                return NotFound();
            }

            return issue;
        }

        // PUT: api/Issues/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIssue(long id, PutIssueDto putIssue)
        {
            var issue = await _context.Issues.Include(i => i.User).SingleOrDefaultAsync(i => i.Id == id);
            
            if (issue == null)
            {
                return BadRequest(new { message = "Issue doesn't exist." });
            }
            else if (GetCurrentUserId() != issue.User.Id)
            {
                return BadRequest(new { message = "Only the creator of an Issue can modify it." });
            }

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
        public async Task<ActionResult<GetIssueDto>> PostIssue(PostIssueDto postIssue)
        {
            var issue = _mapper.Map<Issue>(postIssue);
            issue.User = await _context.Users.FindAsync(GetCurrentUserId());
            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIssue", new { id = issue.Id }, _mapper.Map<GetIssueDto>(issue));
        }

        // DELETE: api/Issues/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<GetIssueDto>> DeleteIssue(long id)
        {
            var issue = await _context.Issues.Include(i => i.User).SingleOrDefaultAsync(i => i.Id == id);
            if (issue == null)
            {
                return NotFound();
            }
            else if (GetCurrentUserId() != issue.User.Id)
            {
                return BadRequest(new { message = "Only the creator of an Issue can delete it." });
            }

            _context.Issues.Remove(issue);
            await _context.SaveChangesAsync();

            return _mapper.Map<GetIssueDto>(issue);
        }

        private bool IssueExists(long id)
        {
            return _context.Issues.Any(e => e.Id == id);
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
        }
    }
}
