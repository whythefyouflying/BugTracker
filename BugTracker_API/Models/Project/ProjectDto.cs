using System;

public class GetProjectDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Issues { get; set; }
    public UserDto User { get; set; }
}

public class PutProjectDto
{
    public string Title { get; set; }
    public string Description { get; set; }
}

public class PostProjectDto
{
    public string Title { get; set; }
    public string Description { get; set; }
}