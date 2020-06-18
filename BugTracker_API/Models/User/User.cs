////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Models\User\User.cs </file>
///
/// <copyright file="User.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the user class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>   An user. </summary>
///
/// <remarks>   Dawid, 18/06/2020. </remarks>
////////////////////////////////////////////////////////////////////////////////////////////////////

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public List<Project> Projects { get; set; }
    public List<Issue> Issues { get; set; }
    public List<Comment> Comments { get; set; }
}