////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Models\Issue\Issue.cs </file>
///
/// <copyright file="Issue.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the issue class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>   An issue. </summary>
///
/// <remarks>   Dawid, 18/06/2020. </remarks>
////////////////////////////////////////////////////////////////////////////////////////////////////

public class Issue
{
    public long Id { get; set; }
    [Required]
    public int Number { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public string Body { get; set; }
    public string Status { get; set; } = "open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Comment> Comments { get; set; }
    public Project Project { get; set; }
    public User User { get; set; }
}