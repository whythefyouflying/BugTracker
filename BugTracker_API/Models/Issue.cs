using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Issue
{
    public long Id { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public string Body { get; set; }
    public string Status { get; set; } = "open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Comment> Comments { get; set; }
    public User User { get; set; }
}