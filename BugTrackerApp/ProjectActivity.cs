using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Xamarin.Essentials;

namespace BugTrackerApp
{
    [Activity(Label = "ProjectActivity", Theme = "@style/AppTheme.NoActionBar", ParentActivity = typeof(MainActivity))]
    public class ProjectActivity : AppCompatActivity
    {
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

            SetContentView(Resource.Layout.activity_project);

            // Create your application here

            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.projectToolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            Title = " ";
        }
    }
}