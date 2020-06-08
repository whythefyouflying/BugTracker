using System;
using System.Collections.Generic;

public class Project
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Issue> Issues { get; set; }
    public User User { get; set; }
}