using Newtonsoft.Json;
using System;
namespace BugTrackerApp.Models
{
    public class ErrorResponse
    {
        public string Message { get; set; }
    }

    public class AuthToken
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }

    public class AuthId
    {
        public int Int { get; set; }
    }

    public class AuthAccountDetails
    {
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}