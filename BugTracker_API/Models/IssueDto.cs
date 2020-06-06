using System;

namespace BugTracker_API.Models
{
    public class GetIssueDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Comments { get; set; }
    }

    public class PostIssueDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
    }

    public class PutIssueDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Status { get; set; }
    }
}
