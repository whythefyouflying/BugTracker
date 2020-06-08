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
    [Route("api/issues/{issueId:int}/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ISharedService _service;

        public CommentsController(DataContext context, IMapper mapper, ISharedService service)
        {
            _context = context;
            _mapper = mapper;
            _service = service;
        }

        [AllowAnonymous]
        // GET: api/Comments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetCommentDto>>> GetComments(long issueId)
        {
            if (!(await _service.GetIssueAsync(issueId) is Issue issue)) return NotFound("Issue not found");

            return await _context.Comments
                .Where(comment => comment.Issue == issue)
                .Include(comment => comment.User)
                .Select(comment => _mapper.Map<GetCommentDto>(comment))
                .ToListAsync();
        }

        [AllowAnonymous]
        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetCommentDto>> GetComment(long issueId, long id)
        {
            //if (!(await GetProjectAsync(projectName) is Project project)) return NotFound("Project not found");
            if (!(await _service.GetIssueAsync(issueId) is Issue issue)) return NotFound("Issue not found");

            return await _context.Comments
                .Where(comment => comment.Issue == issue)
                .Where(comment => comment.Id == id)
                .Include(comment => comment.User)
                .Select(comment => _mapper.Map<GetCommentDto>(comment))
                .SingleOrDefaultAsync();
        }

        // PUT: api/Comments/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(long issueId, long id, PutCommentDto putComment)
        {
            if (!(await _service.GetIssueAsync(issueId) is Issue issue)) return NotFound("Issue not found");

            var comment = await _context.Comments
                .Where(comment => comment.Issue == issue)
                .Where(comment => comment.Id == id)
                .Include(comment => comment.User)
                .SingleOrDefaultAsync();

            if (comment == null) return BadRequest("Comment not found.");
            else if (_service.GetCurrentUserId() != comment.User.Id) return BadRequest("Only the creator of a Comment can modify it.");

            _context.Entry(_mapper.Map(putComment, comment)).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
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

        // POST: api/Comments
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<GetCommentDto>> PostComment(long issueId, PostCommentDto postComment)
        {
            if (!(await _service.GetIssueAsync(issueId) is Issue issue)) return NotFound("Issue not found");

            var comment = _mapper.Map<Comment>(postComment);

            comment.Issue = issue;
            comment.User = await _context.Users.FindAsync(_service.GetCurrentUserId());

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComment", new { id = comment.Id, issueId }, _mapper.Map<GetCommentDto>(comment));
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<GetCommentDto>> DeleteComment(long issueId, long id)
        {
            if (!(await _service.GetIssueAsync(issueId) is Issue issue)) return NotFound("Issue not found");

            var comment = await _context.Comments
                .Where(comment => comment.Issue == issue)
                .Where(comment => comment.Id == id)
                .Include(comment => comment.User)
                .SingleOrDefaultAsync();

            if (comment == null) return BadRequest("Comment not found.");
            else if (_service.GetCurrentUserId() != comment.User.Id) return BadRequest("Only the creator of a Comment can remove it.");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return _mapper.Map<GetCommentDto>(comment);
        }

        private bool CommentExists(long id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}
