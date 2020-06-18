////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Controllers\CommentsController.cs </file>
///
/// <copyright file="CommentsController.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the comments controller class. </summary>
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
    /// <summary>   A controller for handling comments. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    [Authorize]
    [Route("api/projects/{projectId:int}/issues/{issueNumber:int}/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
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

        public CommentsController(DataContext context, IMapper mapper, ISharedService service)
        {
            _context = context;
            _mapper = mapper;
            _service = service;
        }

        [AllowAnonymous]
        // GET: api/Comments

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   (An Action that handles HTTP GET requests) gets the comments. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="issueNumber">  The issue number. </param>
        ///
        /// <returns>   An asynchronous result that yields the comments. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetCommentDto>>> GetComments(long projectId, int issueNumber)
        {
            if (!(await _service.GetProjectAsync(projectId) is Project project)) return NotFound("Project not found.");
            if (!(await _service.GetIssueAsync(project, issueNumber) is Issue issue)) return NotFound("Issue not found.");

            return await _context.Comments
                .Where(comment => comment.Issue == issue)
                .Include(comment => comment.User)
                .Select(comment => _mapper.Map<GetCommentDto>(comment))
                .ToListAsync();
        }

        [AllowAnonymous]
        // GET: api/Comments/5

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   (An Action that handles HTTP GET requests) gets a comment. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="issueNumber">  The issue number. </param>
        /// <param name="id">           The identifier. </param>
        ///
        /// <returns>   An asynchronous result that yields the comment. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpGet("{id}")]
        public async Task<ActionResult<GetCommentDto>> GetComment(long projectId, int issueNumber, long id)
        {
            if (!(await _service.GetProjectAsync(projectId) is Project project)) return NotFound("Project not found.");
            if (!(await _service.GetIssueAsync(project, issueNumber) is Issue issue)) return NotFound("Issue not found.");

            var comment = await _context.Comments
                .Where(comment => comment.Issue == issue)
                .Where(comment => comment.Id == id)
                .Include(comment => comment.User)
                .Select(comment => _mapper.Map<GetCommentDto>(comment))
                .SingleOrDefaultAsync();
            if (comment == null) return NotFound("Comment doesn't exist.");
            return comment;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// PUT: api/Comments/5 To protect from overposting attacks, enable the specific properties you
        /// want to bind to, for more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <exception cref="DbUpdateConcurrencyException"> Thrown when a Database Update Concurrency
        ///                                                 error condition occurs. </exception>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="issueNumber">  The issue number. </param>
        /// <param name="id">           The identifier. </param>
        /// <param name="putComment">   The put comment. </param>
        ///
        /// <returns>   An asynchronous result that yields an IActionResult. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(long projectId, int issueNumber, long id, PutCommentDto putComment)
        {
            if (!(await _service.GetProjectAsync(projectId) is Project project)) return NotFound("Project not found.");
            if (!(await _service.GetIssueAsync(project, issueNumber) is Issue issue)) return NotFound("Issue not found");

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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// POST: api/Comments To protect from overposting attacks, enable the specific properties you
        /// want to bind to, for more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="issueNumber">  The issue number. </param>
        /// <param name="postComment">  The post comment. </param>
        ///
        /// <returns>   An asynchronous result that yields an ActionResult&lt;GetCommentDto&gt; </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public async Task<ActionResult<GetCommentDto>> PostComment(long projectId, int issueNumber, PostCommentDto postComment)
        {
            if (!(await _service.GetProjectAsync(projectId) is Project project)) return NotFound("Project not found.");
            if (!(await _service.GetIssueAsync(project, issueNumber) is Issue issue)) return NotFound("Issue not found");

            var comment = _mapper.Map<Comment>(postComment);

            comment.Issue = issue;
            comment.User = await _context.Users.FindAsync(_service.GetCurrentUserId());

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComment", new { id = comment.Id, issueNumber, projectId }, _mapper.Map<GetCommentDto>(comment));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   DELETE: api/Comments/5. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="issueNumber">  The issue number. </param>
        /// <param name="id">           The identifier. </param>
        ///
        /// <returns>   An asynchronous result that yields an ActionResult&lt;GetCommentDto&gt; </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpDelete("{id}")]
        public async Task<ActionResult<GetCommentDto>> DeleteComment(long projectId, int issueNumber, long id)
        {
            if (!(await _service.GetProjectAsync(projectId) is Project project)) return NotFound("Project not found.");
            if (!(await _service.GetIssueAsync(project, issueNumber) is Issue issue)) return NotFound("Issue not found");

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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Queries if a given comment exists. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="id">   The identifier. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private bool CommentExists(long id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}
