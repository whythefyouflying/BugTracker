using System;

namespace BugTrackerApp.Models
{
    public class Comment
    {
        public long Id { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
    }
}