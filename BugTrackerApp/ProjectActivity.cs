using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using BugTrackerApp.Models;
using BugTrackerApp.Services;
using Refit;
using Xamarin.Essentials;

namespace BugTrackerApp
{
    [Activity(Label = "ProjectActivity", Theme = "@style/AppTheme.NoActionBar", ParentActivity = typeof(MainActivity))]
    public class ProjectActivity : AppCompatActivity
    {
        private readonly IApiService apiService = ApiService.GetApiService();
        private string authToken;
        private long projectId;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            if (savedInstanceState != null)
            {
                authToken = savedInstanceState.GetString("jwt_token", null);
            }
            else
            {
                authToken = Intent.GetStringExtra("jwt_token");
                projectId = Intent.GetLongExtra("project_id", 0L);
            }

            SetContentView(Resource.Layout.activity_project);

            // Create your application here

            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.projectToolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            Title = " ";

            Project project = await GetProject(projectId);
            TextView projectTitle = FindViewById<TextView>(Resource.Id.projectTitleTextView);
            TextView projectDescription = FindViewById<TextView>(Resource.Id.projectDescriptionTextView);

            projectTitle.Text = project.Title;
            projectDescription.Text = project.Description;
        }

        private async Task<Project> GetProject(long projectId)
        {
            try
            {
                return await apiService.GetProject(projectId);
            }
            catch (ApiException ex)
            {
                var statusCode = ex.StatusCode;
                var error = await ex.GetContentAsAsync<ErrorResponse>();
                Toast.MakeText(Application.Context, error.Message, ToastLength.Short).Show();
                return null;
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
                return null;
            }
        }
    }
}