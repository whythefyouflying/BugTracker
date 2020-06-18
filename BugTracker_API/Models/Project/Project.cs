////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Models\Project\Project.cs </file>
///
/// <copyright file="Project.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the project class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

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