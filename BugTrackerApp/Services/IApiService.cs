////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTrackerApp\Services\IApiService.cs </file>
///
/// <copyright file="IApiService.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Declares the IApiService interface. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Threading.Tasks;

using BugTrackerApp.Models;
using Refit;

namespace BugTrackerApp.Services
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Interface for API service. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public interface IApiService
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Posts a login. </summary>
        ///
        /// <param name="loginDetails"> The login details. </param>
        ///
        /// <returns>   An asynchronous result that yields an AuthToken. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [Post("/auth/login")]
        Task<AuthToken> PostLogin([Body] AuthAccountDetails loginDetails);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Posts a register. </summary>
        ///
        /// <param name="registerDetails">  The register details. </param>
        ///
        /// <returns>   An asynchronous result that yields an AuthId. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [Post("/auth/register")]
        Task<AuthId> PostRegister([Body] AuthAccountDetails registerDetails);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the projects. </summary>
        ///
        /// <returns>   An asynchronous result that yields the projects. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [Get("/projects")]
        Task<List<Project>> GetProjects();

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets a project. </summary>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        ///
        /// <returns>   An asynchronous result that yields the project. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [Get("/projects/{id}")]
        Task<Project> GetProject([AliasAs("id")] long projectId);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Posts a project. </summary>
        ///
        /// <param name="projectDetails">   The project details. </param>
        /// <param name="bearerToken">      The bearer token. </param>
        ///
        /// <returns>   An asynchronous result that yields a Project. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [Post("/projects")]
        Task<Project> PostProject([Body] PostProject projectDetails, [Header("Authorization")] string bearerToken);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the issues. </summary>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        ///
        /// <returns>   An asynchronous result that yields the issues. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [Get("/projects/{projectId}/issues")]
        Task<List<Issue>> GetIssues([AliasAs("projectId")] long projectId);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Posts an issue. </summary>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="issueDetails"> The issue details. </param>
        /// <param name="bearerToken">  The bearer token. </param>
        ///
        /// <returns>   An asynchronous result that yields an Issue. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [Post("/projects/{projectId}/issues")]
        Task<Issue> PostIssue([AliasAs("projectId")] long projectId, [Body] PostIssue issueDetails, [Header("Authorization")] string bearerToken);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets an issue. </summary>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="issueNumber">  The issue number. </param>
        ///
        /// <returns>   An asynchronous result that yields the issue. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [Get("/projects/{projectId}/issues/{issueNumber}")]
        Task<Issue> GetIssue(long projectId, int issueNumber);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the comments. </summary>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="issueNumber">  The issue number. </param>
        ///
        /// <returns>   An asynchronous result that yields the comments. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [Get("/projects/{projectId}/issues/{issueNumber}/comments")]
        Task<List<Comment>> GetComments(long projectId, int issueNumber);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Posts a comment. </summary>
        ///
        /// <param name="projectId">        Identifier for the project. </param>
        /// <param name="issueNumber">      The issue number. </param>
        /// <param name="commentDetails">   The comment details. </param>
        /// <param name="bearerToken">      The bearer token. </param>
        ///
        /// <returns>   An asynchronous result that yields a Comment. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [Post("/projects/{projectId}/issues/{issueNumber}/comments")]
        Task<Comment> PostComment(long projectId, int issueNumber, [Body] PostComment commentDetails, [Header("Authorization")] string bearerToken);
    }
}