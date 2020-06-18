////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Models\Issue\IssueDto.cs </file>
///
/// <copyright file="IssueDto.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the issue data transfer object classes. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace BugTracker_API.Models
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A get issue data transfer object. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class GetIssueDto
    {
        public long Id { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Comments { get; set; }
        public UserDto User { get; set; }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A post issue data transfer object. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class PostIssueDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A put issue data transfer object. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class PutIssueDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Status { get; set; }
    }
}
