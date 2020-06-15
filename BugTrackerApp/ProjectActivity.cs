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
                projectId = savedInstanceState.GetLong("project_id");
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

            Project project;
            try
            {
                project = await GetProject(projectId);
                TextView projectOwner = FindViewById<TextView>(Resource.Id.projectOwnerTextView);
                TextView projectTitle = FindViewById<TextView>(Resource.Id.projectTitleTextView);
                TextView projectDescription = FindViewById<TextView>(Resource.Id.projectDescriptionTextView);
                TextView projectIssuesCount = FindViewById<TextView>(Resource.Id.projectIssuesCount);
                LinearLayout projectIssuesButton = FindViewById<LinearLayout>(Resource.Id.projectIssuesButton);

                projectOwner.Text = project.User.Username;
                projectTitle.Text = project.Title;
                projectDescription.Text = project.Description;
                projectIssuesCount.Text = project.Issues.ToString();


                projectIssuesButton.Click += (sender, e) =>
                {
                    var intent = new Intent(this, typeof(IssuesActivity));
                    intent.PutExtra("jwt_token", authToken);
                    intent.PutExtra("project_id", project.Id);
                    StartActivity(intent);
                };
            }
            catch (ApiException ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
            }

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

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString("jwt_token", authToken);
            outState.PutLong("project_id", projectId);
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