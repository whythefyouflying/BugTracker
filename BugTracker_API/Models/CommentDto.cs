using System;

namespace BugTracker_API.Models
{
    public class GetCommentDto
    {
        public long Id { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
    }

    public class PostCommentDto
    {
        public string Body { get; set; }
    }

    public class PutCommentDto
    {
        public string Body { get; set; }
    }
}
