

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using BugTrackerApp.Models;
using BugTrackerApp.Services;
using Xamarin.Essentials;

namespace BugTrackerApp
{
    [Activity(Label = "@string/create_issue", Theme ="@style/AppTheme.NoActionBar", ParentActivity = typeof(IssuesActivity), WindowSoftInputMode = SoftInput.StateVisible)]
    public class CreateIssueActivity : AppCompatActivity
    {
        private readonly IApiService apiService = ApiService.GetApiService();
        private string authToken;
        private long projectId;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            if (savedInstanceState != null)
            {
                authToken = savedInstanceState.GetString("jwt_token", null);
                projectId = savedInstanceState.GetLong("projectId");
            }
            else
            {
                authToken = Intent.GetStringExtra("jwt_token");
                projectId = Intent.GetLongExtra("project_id", 0L);
            }

            SetContentView(Resource.Layout.activity_createissue);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.createIssueToolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            EditText issueTitle = FindViewById<EditText>(Resource.Id.createIssueTitleEditText);

            _ = issueTitle.RequestFocus();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_createissue, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_createIssue)
            {
                CreateIssue();
                return true;
            }
            else if (id == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void CreateIssue()
        {
            EditText issueTitle = FindViewById<EditText>(Resource.Id.createIssueTitleEditText);
            EditText issueBody = FindViewById<EditText>(Resource.Id.createIssueBodyEditText);
            apiService.PostIssue(projectId, new PostIssue { Title = issueTitle.Text, Body = issueBody.Text }, authToken)
                .ContinueWithSuccess(issue => {
                    var intent = new Intent(this, typeof(IssueActivity));
                    intent.PutExtra("jwt_token", authToken);
                    intent.PutExtra("project_id", projectId);
                    intent.PutExtra("issue_number", issue.Number);
                    StartActivity(intent);
                })
                .ContinueWithFailure(ex =>
                {
                    /*if(ex.InnerException is ApiException apiEx)
                    {
                        var statusCode = apiEx.StatusCode;
                        var error = apiEx.GetContentAs<ErrorResponse>();
                        RunOnUiThread(() => Toast.MakeText(context, error?.Message ?? "XD", ToastLength.Long).Show());
                    }*/
                    RunOnUiThread(() => Toast.MakeText(Application.Context, ex.Message, ToastLength.Long).Show());
                });
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString("jwt_token", authToken);
            outState.PutLong("project_id", projectId);
            base.OnSaveInstanceState(outState);
        }
    }
}