using Refit;

namespace BugTrackerApp.Services
{
    public static class ApiService
    {
        public static IApiService apiService;
        static string baseUrl = "https://bugtrackerapi20200605232913.azurewebsites.net/api";
        public static IApiService GetApiService()
        {
            apiService = RestService.For<IApiService>(baseUrl);
            return apiService;
        }
    }
}