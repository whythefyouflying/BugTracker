////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Models\User\UserRegisterDto.cs </file>
///
/// <copyright file="UserRegisterDto.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the user register data transfer object class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

public class UserRegisterDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}