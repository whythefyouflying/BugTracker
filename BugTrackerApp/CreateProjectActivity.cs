using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Resources;
using AndroidX.AppCompat.Widget;
using BugTrackerApp.Models;
using BugTrackerApp.Services;
using Refit;
using Xamarin.Essentials;

namespace BugTrackerApp
{
    [Activity(Label = "@string/create_project", Theme = "@style/AppTheme.NoActionBar", ParentActivity = typeof(MainActivity), WindowSoftInputMode = SoftInput.StateVisible)]
    public class CreateProjectActivity : AppCompatActivity
    {
        private readonly IApiService apiService = ApiService.GetApiService();
        private string authToken;
        protected override void OnCreate(Bundle savedInstanceState)
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
            }

            SetContentView(Resource.Layout.activity_createproject);
            // Create your application here
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.createProjectToolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            EditText projectTitle = FindViewById<EditText>(Resource.Id.projectTitleEditText);
            EditText projectDescription = FindViewById<EditText>(Resource.Id.projectDescriptionEditText);

            _ = projectTitle.RequestFocus();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_createproject, menu);
            return true;
        }

        // TODO: Implement Android Service for calling API
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_createproject)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}