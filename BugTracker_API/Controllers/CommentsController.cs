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

namespace BugTracker_API.Controllers
{
    [Authorize]
    [Route("api/issues/{issueId:int}/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public CommentsController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [AllowAnonymous]
        // GET: api/Comments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetCommentDto>>> GetComments(long issueId)
        {
            if (!IssueExists(issueId))
                return NotFound();

            var comments = await _context.Comments.Include(c => c.Issue).Include(c => c.User).ToListAsync();
            return comments.Where(c => c.Issue.Id == issueId).Select(c => _mapper.Map<GetCommentDto>(c)).ToList();
        }

        [AllowAnonymous]
        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetCommentDto>> GetComment(long issueId, long id)
        {
            if (!IssueExists(issueId))
                return NotFound();

            var comment = await _context.Comments.Include(c => c.Issue).Include(c => c.User).SingleOrDefaultAsync(c => c.Id == id);

            if (comment == null || comment.Issue.Id != issueId)
            {
                return NotFound();
            }

            return _mapper.Map<GetCommentDto>(comment);
        }

        // PUT: api/Comments/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(long issueId, long id, PutCommentDto putComment)
        {
            if (!IssueExists(issueId))
                return NotFound();

            var comment = await _context.Comments.Include(c => c.Issue).SingleOrDefaultAsync(c => c.Id == id);

            if (comment == null || comment.Issue.Id != issueId)
            {
                return BadRequest();
            }

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
            var issue = await _context.Issues.Include(i => i.User).SingleOrDefaultAsync(i => i.Id == issueId);

            if (issue == null)
            {
                return NotFound();
            }

            var comment = _mapper.Map<Comment>(postComment);

            comment.Issue = issue;
            comment.User = await _context.Users.FindAsync(issue.User.Id);

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComment", new { id = comment.Id, issueId }, _mapper.Map<GetCommentDto>(comment));
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<GetCommentDto>> DeleteComment(long issueId, long id)
        {
            if (!IssueExists(issueId))
                return NotFound();

            var comment = await _context.Comments.Include(c => c.Issue).Include(c => c.User).SingleOrDefaultAsync(c => c.Id == id);

            if (comment == null || comment.Issue.Id != issueId)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return _mapper.Map<GetCommentDto>(comment);
        }

        private bool CommentExists(long id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }

        private bool IssueExists(long id)
        {
            return _context.Issues.Any(e => e.Id == id);
        }
    }
}
