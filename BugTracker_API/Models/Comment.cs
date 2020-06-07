using System;
using System.ComponentModel.DataAnnotations;

public class Comment
{ 
    public long Id { get; set; }
    [Required]
    public string Body { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Issue Issue { get; set; }
    public User User { get; set; }
}