using System;

namespace BugTrackerApp.Models
{
    public class Project
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Issues { get; set; }
        public User User { get; set; }
    }

    public class PostProject
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}