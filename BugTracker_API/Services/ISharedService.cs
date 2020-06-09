using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker_API.Services
{
    public interface ISharedService
    {
        public Task<Issue> GetIssueAsync(Project project, long issueId);
        public int GetCurrentUserId();
        Task<Project> GetProjectAsync(long projectId);
    }
}
