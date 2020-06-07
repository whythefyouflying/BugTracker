using System.Collections.Generic;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public List<Issue> Issues { get; set; }
    public List<Comment> Comments { get; set; }
}