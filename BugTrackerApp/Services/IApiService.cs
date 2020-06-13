using System.Threading.Tasks;

using BugTrackerApp.Models;
using Java.Lang;
using Refit;

namespace BugTrackerApp.Services
{
    public interface IApiService
    {
        [Post("/auth/login")]
        Task<AuthToken> PostLogin([Body] AuthAccountDetails loginDetails);

        [Post("/auth/register")]
        Task<AuthId> PostRegister([Body] AuthAccountDetails registerDetails);
    }
}