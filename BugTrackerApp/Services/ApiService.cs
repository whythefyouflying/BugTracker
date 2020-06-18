////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTrackerApp\Services\ApiService.cs </file>
///
/// <copyright file="ApiService.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the API service class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using Refit;

namespace BugTrackerApp.Services
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A service for accessing apis information. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public static class ApiService
    {
        /// <summary>   The API service. </summary>
        public static IApiService apiService;
        /// <summary>   The base URL for the API. </summary>
        static string baseUrl = "https://bugtrackerapi20200605232913.azurewebsites.net/api";

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets API service. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <returns>   The API service. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static IApiService GetApiService()
        {
            apiService = RestService.For<IApiService>(baseUrl);
            return apiService;
        }
    }
}