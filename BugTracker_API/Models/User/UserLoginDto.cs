////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Models\User\UserLoginDto.cs </file>
///
/// <copyright file="UserLoginDto.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the user login data transfer object class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

public class UserLoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}