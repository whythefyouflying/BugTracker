﻿using System.Collections.Generic;
using System.Threading.Tasks;

using BugTrackerApp.Models;
using Refit;

namespace BugTrackerApp.Services
{
    public interface IApiService
    {
        [Post("/auth/login")]
        Task<AuthToken> PostLogin([Body] AuthAccountDetails loginDetails);

        [Post("/auth/register")]
        Task<AuthId> PostRegister([Body] AuthAccountDetails registerDetails);

        [Get("/projects")]
        Task<List<Project>> GetProjects();
    }
}