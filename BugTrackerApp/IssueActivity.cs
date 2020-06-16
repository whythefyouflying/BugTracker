using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text.Format;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using BugTrackerApp.Models;
using BugTrackerApp.Services;
using Google.Android.Material.Chip;
using Java.Lang;
using Refit;
using Xamarin.Essentials;

namespace BugTrackerApp
{
    [Activity(Label = "IssueActivity", Theme = "@style/AppTheme.NoActionBar", ParentActivity = typeof(MainActivity))]
    public class IssueActivity : AppCompatActivity
    {

        private readonly IApiService apiService = ApiService.GetApiService();
        private string authToken;
        private long projectId;
        private int issueNumber;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            if (savedInstanceState != null)
            {
                authToken = savedInstanceState.GetString("jwt_token", null);
                projectId = savedInstanceState.GetLong("project_id");
                issueNumber = savedInstanceState.GetInt("issue_number");
            }
            else
            {
                authToken = Intent.GetStringExtra("jwt_token");
                projectId = Intent.GetLongExtra("project_id", 0L);
                issueNumber = Intent.GetIntExtra("issue_number", 0);
            }

            SetContentView(Resource.Layout.activity_issue);

            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.issueToolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            Title = " ";

            // Create your application here

            Issue issue;

            try
            {
                issue = await GetIssue(projectId, issueNumber);

                TextView issueNumberText = FindViewById<TextView>(Resource.Id.issueNumber);
                Chip issueStatus = FindViewById<Chip>(Resource.Id.issueStatus);
                TextView issueCreationTime = FindViewById<TextView>(Resource.Id.issueCreationTime);
                TextView issueTitle = FindViewById<TextView>(Resource.Id.issueTitleTextView);
                TextView issueBody = FindViewById<TextView>(Resource.Id.issueBodyTextView);

                issueNumberText.Text = $"#{issue.Number}";
                issueStatus.Text = issue.Status;

                long now = DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeMilliseconds();
                long createdAt = ((DateTimeOffset)issue.CreatedAt.ToLocalTime()).ToUnixTimeMilliseconds();
                
                issueCreationTime.Text = DateUtils.GetRelativeTimeSpanString(createdAt, now, DateUtils.MinuteInMillis);

                issueTitle.Text = issue.Title;
                issueBody.Text = issue.Body;
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
            }
  
        }

        private async Task<Issue> GetIssue(long projectId, int issueNumber)
        {
            try
            {
                return await apiService.GetIssue(projectId, issueNumber);
            }
            catch (ApiException ex)
            {
                var statusCode = ex.StatusCode;
                var error = await ex.GetContentAsAsync<ErrorResponse>();
                Toast.MakeText(Application.Context, error.Message, ToastLength.Short).Show();
                Finish();
                return null;
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
                Finish();
                return null;
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString("jwt_token", authToken);
            outState.PutLong("project_id", projectId);
            outState.PutInt("issue_number", issueNumber);
            base.OnSaveInstanceState(outState);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}