////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Models\Comment\Comment.cs </file>
///
/// <copyright file="Comment.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the comment class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel.DataAnnotations;

////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>   A comment. </summary>
///
/// <remarks>   Dawid, 18/06/2020. </remarks>
////////////////////////////////////////////////////////////////////////////////////////////////////

public class Comment
{ 
    public long Id { get; set; }
    [Required]
    public string Body { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Issue Issue { get; set; }
    public User User { get; set; }
}