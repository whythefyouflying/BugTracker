using System;

namespace BugTrackerApp.Models
{
    public class Issue
    {
        public long Id { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Comments { get; set; }
        public User User { get; set; }
    }
}